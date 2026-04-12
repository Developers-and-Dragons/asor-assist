namespace AsorAssistant.Domain.Models;

public static class KnownProviders
{
    public static IReadOnlyList<(string Name, string Id)> All { get; } =
    [
        ("Self-Built", "SELF-BUILT"),
        ("Payslip Ltd.", "PAYSLIP_LTD"),
        ("HireVue, Inc.", "HIREVUE_INC"),
        ("Uplimit", "UPLIMIT"),
        ("Aisera", "AISERA"),
        ("Kainos", "KAINOS"),
        ("Box", "BOX"),
        ("Avalara, Inc.", "AVALARA_INC"),
        ("MoSeeker, Inc.", "MOSEEKER_INC"),
        ("Laurel.AI", "LAURELAI"),
        ("Censia Inc.", "CENSIA_INC"),
        ("Techwolf", "TECHWOLF"),
        ("Galileo", "GALILEO"),
        ("Auditoria.ai", "AUDITORIAAI"),
        ("Workboard Inc.", "WORKBOARD_INC"),
        ("Compa Technologies, Inc.", "COMPA_TECHNOLOGIES_INC"),
    ];
}
