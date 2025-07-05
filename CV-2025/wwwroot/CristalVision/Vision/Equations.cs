using CV_2025.CristalVision.Database;
using static CV_2025.CristalVision.Vision.Page;

namespace CV_2025.CristalVision.Vision
{
    public abstract class Equation
    {
        public enum Format { Fraction, Script, Integral, LargeOperator, Function, Accent, LimitAndLog, Operator, Matrix }
        public Format format;
    }

    public sealed class Fraction : Equation
    {
        public List<Character> numerator, denominator;
    }

    public class Equations
    {
        public static List<Fraction> GetFractions(List<Character> unknownChars, List<Character> knownChars)
        {
            List<Character> lines = [.. unknownChars.Where(character => character.Width / character.Height > 10)];

            List<Fraction> fractions = [];
            foreach (Character line in lines)
            {
                List<Character> numerator = [.. knownChars.Where(character => (line.Top - character.Bottom > 0) && (line.Top - character.Bottom < 50) && character.Left >= line.Left && character.Right <= line.Right)];
                List<Character> denominator = [.. knownChars.Where(character => (character.Top - line.Bottom > 0) && (character.Top - line.Bottom < 50) && character.Left >= line.Left && character.Right <= line.Right)];

                Fraction fraction = new() { format = Equation.Format.Fraction, numerator = numerator, denominator = denominator };
                fractions.Add(fraction);
            }

            foreach (Fraction fraction in fractions)
            {
                if (fraction.format == Equation.Format.Fraction)
                {
                    foreach (Character character in fraction.numerator)
                        knownChars.Remove(character);

                    foreach (Character character in fraction.denominator)
                        knownChars.Remove(character);
                }
            }

            return fractions;
        }
    }
}
