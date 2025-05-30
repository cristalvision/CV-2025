using static CV_2025.CristalVision.Vision.Page;

namespace CV_2025.CristalVision.Vision
{
    public struct Equation
    {
        public enum Format { Fraction, Script, Integral, LargeOperator, Function, Accent, LimitAndLog, Operator, Matrix }
        public Format format;
        public List<Character> numerator, denominator;
    }

    public class Equations
    {
        public static List<Equation> GetFractions(List<Character> unknownChars, List<Character> knownChars)
        {
            List<Character> lines = [.. unknownChars.Where(character => character.Width / character.Height > 10)];

            foreach (Character line in lines)
            {
                List<Character> numerator = knownChars.Where(character => (line.Top - character.Bottom > 0) && (line.Top - character.Bottom < 50) && character.Left >= line.Left && character.Right <= line.Right).ToList();
                List<Character> denominator = knownChars.Where(character => (character.Top - line.Bottom > 0) && (character.Top - line.Bottom < 50) && character.Left >= line.Left && character.Right <= line.Right).ToList();

                Equation equation = new() { format = Equation.Format.Fraction, numerator = numerator, denominator = denominator };

            }

            return [];
        }
    }
}
