namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Captures which ZATCA API to use
    /// </summary>
    public enum Env
    {
        Sandbox,
        Simulation,
        Production
    }

    public static class EnvUtils
    {
        public static Env Parse(string envName) => envName switch
        {
            "Sandbox" => Env.Sandbox,
            "Simulation" => Env.Simulation,
            "Production" => Env.Production,
            _ => throw new InvalidOperationException($"Unrecognized ZatcaEnvironment {envName}."),
        };
    }
}
