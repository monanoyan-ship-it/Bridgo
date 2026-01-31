namespace Bridgo.DTOs.Evrim;

/// <summary>
/// Evrim beyanname olusturma istegi
/// </summary>
public class EvrimDeclarationCreateRequest
{
    /// <summary>
    /// Dosya tipi: IHR (Ihracat), ITH (Ithalat), ETGB
    /// </summary>
    public string DosyaTipi { get; set; } = "IHR";

    /// <summary>
    /// Rejim kodu (4 haneli)
    /// Ornek: 1000 (Serbest Dolasim), 1040 (Dahilde Isleme), 3151 (Gecici Ihracat)
    /// </summary>
    public string RejimKodu { get; set; } = string.Empty;

    /// <summary>
    /// Gumruk idaresi kodu (4 haneli)
    /// </summary>
    public string GumrukIdaresiKodu { get; set; } = string.Empty;

    /// <summary>
    /// Ihracatci/Satici firma bilgileri
    /// </summary>
    public EvrimDeclarationCompanyDto Ihracatci { get; set; } = new();

    /// <summary>
    /// Ithalatci/Alici firma bilgileri
    /// </summary>
    public EvrimDeclarationCompanyDto Ithalatci { get; set; } = new();

    /// <summary>
    /// Gondericici (Ihracatta - satici, Ithalatta - yurt disi gonderici)
    /// </summary>
    public EvrimDeclarationCompanyDto? Gonderici { get; set; }

    /// <summary>
    /// Alici (Ihracatta - yurt disi alici, Ithalatta - alici firma)
    /// </summary>
    public EvrimDeclarationCompanyDto? Alici { get; set; }

    /// <summary>
    /// Gumruk musaviri bilgileri
    /// </summary>
    public EvrimDeclarationBrokerDto? Musavir { get; set; }

    /// <summary>
    /// Teslim sekli (Incoterm): EXW, FOB, CIF, DDP, vb.
    /// </summary>
    public string TeslimSekli { get; set; } = "FOB";

    /// <summary>
    /// Teslim yeri
    /// </summary>
    public string? TeslimYeri { get; set; }

    /// <summary>
    /// Tasima sekli kodu
    /// 1: Deniz, 2: Demiryolu, 3: Karayolu, 4: Havayolu, 5: Posta, 7: Boru hatti, 9: Diger
    /// </summary>
    public int TasimaSekliKodu { get; set; }

    /// <summary>
    /// Tasima araci kimlik bilgisi (Plaka/IMO/Ucus no)
    /// </summary>
    public string? TasimaAraciKimlik { get; set; }

    /// <summary>
    /// Sinirda tasima araci kimlik bilgisi
    /// </summary>
    public string? SinirdaAracKimlik { get; set; }

    /// <summary>
    /// Konteyner var mi
    /// </summary>
    public bool KonteynerVar { get; set; }

    /// <summary>
    /// Konteyner numaralari (virgullle ayrilmis)
    /// </summary>
    public string? KonteynerNolari { get; set; }

    /// <summary>
    /// Cikis/Giris gumruk idaresi kodu
    /// </summary>
    public string? CikisGumrukIdaresiKodu { get; set; }

    /// <summary>
    /// Varis ulke kodu (2 haneli ISO)
    /// </summary>
    public string VarisUlkeKodu { get; set; } = string.Empty;

    /// <summary>
    /// Odeme sekli kodu
    /// </summary>
    public string? OdemeSekliKodu { get; set; }

    /// <summary>
    /// Toplam fatura tutari
    /// </summary>
    public decimal ToplamFaturaTutar { get; set; }

    /// <summary>
    /// Fatura dovizi
    /// </summary>
    public string FaturaDovizi { get; set; } = "USD";

    /// <summary>
    /// Toplam brut agirlik (kg)
    /// </summary>
    public decimal ToplamBrutAgirlik { get; set; }

    /// <summary>
    /// Toplam net agirlik (kg)
    /// </summary>
    public decimal ToplamNetAgirlik { get; set; }

    /// <summary>
    /// Toplam kap adedi
    /// </summary>
    public int ToplamKapAdedi { get; set; }

    /// <summary>
    /// Beyanname kalemleri
    /// </summary>
    public List<EvrimDeclarationItemDto> Kalemler { get; set; } = new();

    /// <summary>
    /// Ek aciklamalar
    /// </summary>
    public string? Aciklamalar { get; set; }
}

/// <summary>
/// Evrim beyanname olusturma yaniti
/// </summary>
public class EvrimDeclarationCreateResponse
{
    /// <summary>
    /// Islem basarili mi
    /// </summary>
    public bool Basarili { get; set; }

    /// <summary>
    /// Evrim dosya numarasi (Evrim'in verdigi)
    /// </summary>
    public string? DosyaNo { get; set; }

    /// <summary>
    /// Dosya tipi (IHR, ITH, ETGB)
    /// </summary>
    public string? DosyaTipi { get; set; }

    /// <summary>
    /// Beyanname numarasi (tescil sonrasi)
    /// </summary>
    public string? BeyannameNo { get; set; }

    /// <summary>
    /// Tescil tarihi
    /// </summary>
    public DateTime? TescilTarihi { get; set; }

    /// <summary>
    /// Hata mesaji
    /// </summary>
    public string? HataMesaji { get; set; }

    /// <summary>
    /// Hata kodu
    /// </summary>
    public string? HataKodu { get; set; }

    /// <summary>
    /// Uyari mesajlari
    /// </summary>
    public List<string>? Uyarilar { get; set; }
}

/// <summary>
/// Evrim beyanname durum sorgulama yaniti
/// </summary>
public class EvrimDeclarationStatusResponse
{
    /// <summary>
    /// Islem basarili mi
    /// </summary>
    public bool Basarili { get; set; }

    /// <summary>
    /// Evrim dosya numarasi
    /// </summary>
    public string? DosyaNo { get; set; }

    /// <summary>
    /// Dosya tipi
    /// </summary>
    public string? DosyaTipi { get; set; }

    /// <summary>
    /// Beyanname numarasi
    /// </summary>
    public string? BeyannameNo { get; set; }

    /// <summary>
    /// Beyanname durumu
    /// TASLAK, BEKLEMEDE, TESCILLI, KAPALI, HATA
    /// </summary>
    public string? Durum { get; set; }

    /// <summary>
    /// Durum aciklamasi
    /// </summary>
    public string? DurumAciklama { get; set; }

    /// <summary>
    /// Tescil tarihi
    /// </summary>
    public DateTime? TescilTarihi { get; set; }

    /// <summary>
    /// Kapanis tarihi
    /// </summary>
    public DateTime? KapanisTarihi { get; set; }

    /// <summary>
    /// Gumruk idaresi adi
    /// </summary>
    public string? GumrukIdaresiAdi { get; set; }

    /// <summary>
    /// Odenmesi gereken vergiler
    /// </summary>
    public decimal? OdenecekVergi { get; set; }

    /// <summary>
    /// Odenen vergi tutari
    /// </summary>
    public decimal? OdenenVergi { get; set; }

    /// <summary>
    /// Hata mesaji
    /// </summary>
    public string? HataMesaji { get; set; }

    /// <summary>
    /// Son guncelleme tarihi
    /// </summary>
    public DateTime? SonGuncellemeTarihi { get; set; }
}

/// <summary>
/// ETGB (Mikro Ihracat) ozel istegi
/// </summary>
public class EvrimEtgbCreateRequest
{
    /// <summary>
    /// ETGB tipi: 1: Posta, 2: Hizli Kargo
    /// </summary>
    public int EtgbTipi { get; set; } = 2;

    /// <summary>
    /// Gonderici bilgileri
    /// </summary>
    public EvrimDeclarationCompanyDto Gonderici { get; set; } = new();

    /// <summary>
    /// Alici bilgileri
    /// </summary>
    public EvrimDeclarationCompanyDto Alici { get; set; } = new();

    /// <summary>
    /// Varis ulke kodu
    /// </summary>
    public string VarisUlkeKodu { get; set; } = string.Empty;

    /// <summary>
    /// Kargo firması kodu
    /// </summary>
    public string? KargoFirmasiKodu { get; set; }

    /// <summary>
    /// Kargo takip numarasi
    /// </summary>
    public string? KargoTakipNo { get; set; }

    /// <summary>
    /// Toplam fatura tutari
    /// </summary>
    public decimal ToplamFaturaTutar { get; set; }

    /// <summary>
    /// Fatura dovizi
    /// </summary>
    public string FaturaDovizi { get; set; } = "USD";

    /// <summary>
    /// Toplam agirlik (kg)
    /// </summary>
    public decimal ToplamAgirlik { get; set; }

    /// <summary>
    /// Kalemler
    /// </summary>
    public List<EvrimDeclarationItemDto> Kalemler { get; set; } = new();
}
