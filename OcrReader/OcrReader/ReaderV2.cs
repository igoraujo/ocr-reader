using System.Text.RegularExpressions;
using System.Drawing;
using Tesseract;

namespace OcrReader
{
    public class ReaderV2
    {
        static ReaderV2()
        {
        }

        public static void ProcessImage(string imagePath)
        {
            try
            {
                // Pré-processamento da imagem
                string preprocessedImagePath = PreprocessImage(imagePath);

                string tessDataPath = @"C:\Users\igor.araujo\source\repos\ocr-reader\tessdata";

                using var engine = new TesseractEngine(tessDataPath, "por", EngineMode.Default);
                using var img = Pix.LoadFromFile(preprocessedImagePath);
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

        // Pré-processamento da imagem para melhorar precisão
        static string PreprocessImage(string imagePath)
        {
            var originalImage = new Bitmap(imagePath);
            var grayscaleImage = new Bitmap(originalImage.Width, originalImage.Height);

            // Converter para escala de cinza
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color originalColor = originalImage.GetPixel(x, y);
                    int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    Color grayColor = Color.FromArgb(grayScale, grayScale, grayScale);
                    grayscaleImage.SetPixel(x, y, grayColor);
                }
            }

            // Ajustar o contraste (exemplo simples)
            float contrast = 1.2f;
            float adjustment = 0.5f * (1.0f - contrast);
            for (int x = 0; x < grayscaleImage.Width; x++)
            {
                for (int y = 0; y < grayscaleImage.Height; y++)
                {
                    Color grayColor = grayscaleImage.GetPixel(x, y);
                    int adjustedGray = (int)(contrast * grayColor.R + adjustment);
                    adjustedGray = Math.Max(0, Math.Min(255, adjustedGray)); // Limita de 0 a 255
                    grayscaleImage.SetPixel(x, y, Color.FromArgb(adjustedGray, adjustedGray, adjustedGray));
                }
            }

            string preprocessedImagePath = "preprocessed_image.jpg";
            grayscaleImage.Save(preprocessedImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return preprocessedImagePath;
        }

        static bool IsDriverLicense(string text)
        {
            return text.Contains("Carteira Nacional de Habilitação".ToUpper()) || text.Contains("CNH");
        }

        static void ExtractDriverLicenseData(string text)
        {
            Console.WriteLine("Extraindo dados da CNH...");

            // Extrair Nome
            var nomeMatch = Regex.Match(text, @"NOME\s*\n\s*(\d+)");
            var nome = nomeMatch.Success ? nomeMatch.Value.Trim() : "Nome não encontrado";
            Console.WriteLine("Nome: " + nome);

            // Extrair e Validar CPF
            var cpfMatch = Regex.Match(text, @"\b\d{3}\.\d{3}\.\d{3}-\d{2}\b");
            var cpf = cpfMatch.Success && IsValidCPF(cpfMatch.Value) ? cpfMatch.Value : "CPF inválido ou não encontrado";
            Console.WriteLine("CPF: " + cpf);

            // Extrair e Validar Data de Nascimento
            var nascimentoMatch = Regex.Match(text, @"DATA NASCIMENTO\s*\n\s*\d{3}\.\d{3}\.\d{3}-\d{2}\|\s*(\d{2}/\d{2}/\d{4})");
            var dataNascimento = nascimentoMatch.Success && IsValidDate(nascimentoMatch.Groups[1].Value) ? nascimentoMatch.Groups[1].Value : "Data de nascimento inválida ou não encontrada";
            Console.WriteLine("Data de Nascimento: " + dataNascimento);

            // Extrair Número de Registro
            var registroMatch = Regex.Match(text, @"Nº REGISTRO\s*\n\s*(\d+)");
            var registro = registroMatch.Success ? registroMatch.Groups[1].Value : "Número de registro não encontrado";
            Console.WriteLine("Número de Registro: " + registro);

            // Extrair e Validar Data de Validade
            var validadeMatch = Regex.Match(text, @"HABILITAÇÃO\s*\n\s*(\d+)");
            var validade = validadeMatch.Success && IsValidDate(validadeMatch.Value) ? validadeMatch.Value : "Data de validade inválida ou não encontrada";
            Console.WriteLine("Data de Validade: " + validade);
        }

        // Função para validação de CPF
        static bool IsValidCPF(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11 || long.TryParse(cpf, out _) == false) return false;

            for (int j = 0; j < 10; j++)
                if (new string(j.ToString()[0], 11) == cpf) return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            tempCpf += resto;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            tempCpf += resto;

            return cpf.EndsWith(resto.ToString());
        }

        // Função para validação de data
        static bool IsValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }
    }
}
