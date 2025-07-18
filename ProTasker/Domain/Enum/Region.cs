using System.ComponentModel;

namespace ProTasker.Domain.Enum;
public enum Region
{
    [Description("Toshkent shahri")]
    Tashkent = 1,

    [Description("Toshkent viloyati")]
    Tashkentregion,

    [Description("Samarqand")]
    Samarkand,

    [Description("Buxoro")]
    Bukhara,

    [Description("Fargʻona")]
    Fergana,

    [Description("Andijon")]
    Andijan,

    [Description("Namangan")]
    Namangan,

    [Description("Xorazm")]
    Khorezm,

    [Description("Surxondaryo")]
    Surkhandarya,

    [Description("Qashqadaryo")]
    Kashkadarya,

    [Description("Navoiy")]
    Navoi,

    [Description("Jizzax")]
    Jizzakh,

    [Description("Qoraqalpogʻiston Respublikasi")]
    Karakalpakstan
}
