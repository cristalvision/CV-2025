using CV_2025.CristalVision.Vision;

namespace CV_2025.wwwroot.CristalVision.Output
{
    public class Word
    {
        Page page;

        public Word(Page page) 
        {
            this.page = page;
        }

        public void Save() 
        {
            //=====TEST=====
            // Apelarea funcției asincrone

            /*using (WordprocessingDocument wordDoc = WordprocessingDocument.Create("exemplu.docx", DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                Paragraph para = new Paragraph(new Run(new Text("Salut, lume!")));
                body.Append(para);
                mainPart.Document.Append(body);
            }*/
            //=====TEST=====
        }
    }
}
