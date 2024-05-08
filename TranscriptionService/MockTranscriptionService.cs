using System;

namespace TranscriptsProcessor.TranscriptionService
{
    public class MockTranscriptionService
    {
        private static readonly string[] PredefinedTexts = new string[]
        {
            "Texto de ejemplo uno",
            "Texto de ejemplo dos",
            "Texto de ejemplo tres",
            "Texto de ejemplo cuatro"
        };

        public string Transcribe(string mp3File)
        {
            // Simula un error el 5% de las veces
            if (_random.NextDouble() < 0.05)
            {
                throw new Exception("Error genérico en la transcripción.");
            }

            // Selecciona uno de los textos predefinidos al azar
            int index = _random.Next(PredefinedTexts.Length);
            return PredefinedTexts[index];
        }

        private readonly Random _random = new Random();
    }
}
