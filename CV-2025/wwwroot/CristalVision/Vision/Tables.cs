using CV_2025.CristalVision.Database;

namespace CV_2025.CristalVision.Vision
{
    public struct Table
    {
    }

    public class Tables
    {
        Monochrome monochrome;

        /// <summary>
        /// Monochrome image as stream
        /// </summary>
        public Tables(Page page)
        {
            monochrome = page.monochrome;
            /*database = new MySQL("cvtable");
            this.monochrome = monochrome;
            Size = monochrome.Size;
            Content = monochrome.Content;*/
        }
    }
}
