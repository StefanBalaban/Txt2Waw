using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using NAudio.Wave;


System.Console.WriteLine("Input Azure key: ");
var key = Console.ReadLine();
string text = System.IO.File.ReadAllText("./in/in.txt");

await SynthesisToSpeakerAsync(text);


async Task SynthesisToSpeakerAsync(string text)
{
    var speechConfig = SpeechConfig.FromSubscription(key, "eastus");

    int i = 1;
    List<SpeechSynthesisResult> speechSynthesisResults = new List<SpeechSynthesisResult>();
    while (text.Length > 3000)
    {        
        System.Console.WriteLine($"Processing {i}");
        using var audioConfig = AudioConfig.FromWavFileOutput($"./out/out{i}.wav");      

        using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
        speechSynthesisResults.Add(await synthesizer.SpeakTextAsync(text.Substring(0, 3000)));
        text = text.Substring(3000);
        i++;
    }

    
    if (text.Length > 0)
    {
        using var audioConfig = AudioConfig.FromWavFileOutput($"./out/out{i}.wav");      

        using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
        speechSynthesisResults.Add(await synthesizer.SpeakTextAsync(text));
        i++;
    }

    if (speechSynthesisResults.All(result => result.Reason == ResultReason.SynthesizingAudioCompleted))
    {
        Console.WriteLine("Speech synthesis succeeded.");
    }
    else
    {
        Console.WriteLine("Speech synthesis failed. Reasons:");

        foreach (var result in speechSynthesisResults.Where(result => result.Reason != ResultReason.SynthesizingAudioCompleted))
        {
            Console.WriteLine(result.Reason);
        }
    }

    string date = DateTime.Now.ToString("yyyyMMdd-HHmmss");
    using (var writer = new WaveFileWriter($"./out/out{date}.wav", new WaveFormat(16000, 1)))
    {
        for (int j = 1; j < i; j++)
        {
            using (var reader = new WaveFileReader($"./out/out{j}.wav"))
            {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                writer.Write(buffer, 0, buffer.Length);
            }
        }
    }

    for (int j = 1; j < i; j++)
    {
        System.IO.File.Delete($"./out/out{j}.wav");
    }

}