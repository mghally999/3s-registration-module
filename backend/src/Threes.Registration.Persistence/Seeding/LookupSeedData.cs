using Threes.Registration.Domain.Lookups;

namespace Threes.Registration.Persistence.Seeding;

// the lookup seed shipped with the migrations. ids are hard-coded and stable so
// the city -> governorate foreign keys always line up and re-running the seed
// is idempotent. a small but real slice of egyptian governorates and cities.
public static class LookupSeedData
{
    public static readonly Governorate[] Governorates =
    {
        new(1, "Cairo"),
        new(2, "Giza"),
        new(3, "Alexandria"),
        new(4, "Dakahlia"),
        new(5, "Sharqia"),
        new(6, "Qalyubia"),
        new(7, "Port Said"),
        new(8, "Aswan"),
    };

    public static readonly City[] Cities =
    {
        // cairo
        new(101, 1, "Nasr City"),
        new(102, 1, "Maadi"),
        new(103, 1, "Heliopolis"),
        new(104, 1, "Zamalek"),
        new(105, 1, "New Cairo"),

        // giza
        new(201, 2, "Dokki"),
        new(202, 2, "Mohandessin"),
        new(203, 2, "6th of October"),
        new(204, 2, "Haram"),
        new(205, 2, "Sheikh Zayed"),

        // alexandria
        new(301, 3, "Smouha"),
        new(302, 3, "Miami"),
        new(303, 3, "Stanley"),
        new(304, 3, "Sidi Gaber"),

        // dakahlia
        new(401, 4, "Mansoura"),
        new(402, 4, "Mit Ghamr"),
        new(403, 4, "Talkha"),

        // sharqia
        new(501, 5, "Zagazig"),
        new(502, 5, "Belbeis"),
        new(503, 5, "10th of Ramadan"),

        // qalyubia
        new(601, 6, "Banha"),
        new(602, 6, "Shubra El Kheima"),
        new(603, 6, "Qalyub"),

        // port said
        new(701, 7, "Port Fouad"),
        new(702, 7, "El Manakh"),

        // aswan
        new(801, 8, "Aswan"),
        new(802, 8, "Kom Ombo"),
        new(803, 8, "Edfu"),
    };
}
