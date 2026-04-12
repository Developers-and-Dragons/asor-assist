namespace AsorAssistant.Domain.Models;

public static class KnownProviders
{
    public static IReadOnlyList<(string Name, string Id)> All { get; } =
    [
        ("Self-Built", "Provider=SELF-BUILT"),
        ("Payslip Ltd.", "Provider=PAYSLIP_LTD"),
        ("HireVue, Inc.", "Provider=HIREVUE_INC"),
        ("Uplimit", "Provider=UPLIMIT"),
        ("Aisera", "Provider=AISERA"),
        ("Kainos", "Provider=KAINOS"),
        ("Box", "Provider=BOX"),
        ("Avalara, Inc.", "Provider=AVALARA_INC"),
        ("MoSeeker, Inc.", "Provider=MOSEEKER_INC"),
        ("Laurel.AI", "Provider=LAURELAI"),
        ("Censia Inc.", "Provider=CENSIA_INC"),
        ("Techwolf", "Provider=TECHWOLF"),
        ("Galileo", "Provider=GALILEO"),
        ("Auditoria.ai", "Provider=AUDITORIAAI"),
        ("Workboard Inc.", "Provider=WORKBOARD_INC"),
        ("Compa Technologies, Inc.", "Provider=COMPA_TECHNOLOGIES_INC"),
    ];
}
