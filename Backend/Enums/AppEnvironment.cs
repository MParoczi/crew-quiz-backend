using System.ComponentModel;

namespace Backend.Enums;

public enum AppEnvironment
{
    [Description("DEV")]
    Development,

    [Description("PROD")]
    Production
}