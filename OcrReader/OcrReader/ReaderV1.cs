using System.Text.RegularExpressions;
using Tesseract;

namespace OcrReader
{
    public class ReaderV1
    {
        static ReaderV1()
        {
        }

        public static void ProcessImage(string imagePath)
        {
            try
            {
                string tessDataPath = @"C:\Users\igor.araujo\OneDrive - BANCO BMG S A\Documentos\tessdata";

                using var engine = new TesseractEngine(tessDataPath, "por", EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);

                string ocrText = page.GetText();
                Console.WriteLine("Texto OCR Extraído:");
                Console.WriteLine(ocrText);

                if (IsDriverLicense(ocrText))
                {
                    ExtractDriverLicenseData(ocrText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao processar a imagem: " + ex.Message);
            }
        }

        static bool IsDriverLicense(string text)
        {
            return text.Contains("Carteira Nacional de Habilitação".ToUpper()) || text.Contains("CNH");
        }

        static void ExtractDriverLicenseData(string text)
        {
            Console.WriteLine("Extraindo dados da CNH...");

            // Extrair Nome
            var nomeMatch = Regex.Match(text, @"(?<=NOME\s*:\s*)([A-Za-z\s]+)");
            var nome = nomeMatch.Success ? nomeMatch.Value.Trim() : "Nome não encontrado";
            Console.WriteLine("Nome: " + nome);

            // Extrair CPF
            var cpfMatch = Regex.Match(text, @"\b\d{3}\.\d{3}\.\d{3}-\d{2}\b");
            var cpf = cpfMatch.Success ? cpfMatch.Value : "CPF não encontrado";
            Console.WriteLine("CPF: " + cpf);

            // Extrair Data de Nascimento
            var nascimentoMatch = Regex.Match(text, @"(?<=DATA NASCIMENTO\s*:\s*)(\d{2}/\d{2}/\d{4})");
            var dataNascimento = nascimentoMatch.Success ? nascimentoMatch.Value : "Data de nascimento não encontrada";
            Console.WriteLine("Data de Nascimento: " + dataNascimento);

            // Extrair Número de Registro
            var registroMatch = Regex.Match(text, @"(?<=REGISTRO\s*:\s*)(\d+)");
            var registro = registroMatch.Success ? registroMatch.Value : "Número de registro não encontrado";
            Console.WriteLine("Número de Registro: " + registro);

            // Extrair Data de Validade
            var validadeMatch = Regex.Match(text, @"(?<=VALIDADE\s*:\s*)(\d{2}/\d{2}/\d{4})");
            var validade = validadeMatch.Success ? validadeMatch.Value : "Data de validade não encontrada";
            Console.WriteLine("Data de Validade: " + validade);
        }
    }
}
