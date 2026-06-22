namespace Infrastructure.Services;

internal class PythonApiResponse
{
    public bool Is_Ai { get; set; }
    public double Ai_Probability { get; set; }
    public double Confidence { get; set; }
    public string Language { get; set; }
}
