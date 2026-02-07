-- Missing States/Provinces Insert Script
-- Generated: 2026-02-07
-- Only countries with genuine self-governing subdivisions (elected governors/parliaments)
-- NOT including unitary states where governors are centrally appointed

BEGIN;

-- =============================================
-- 🇬🇧 UNITED KINGDOM (Id: 238) - Constituent Countries (own parliaments/assemblies)
-- =============================================
INSERT INTO "States" ("CountryId", "Name", "Code", "Type", "DisplayOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(238, 'England', 'ENG', 'Country', 1, true, NOW(), false),
(238, 'Scotland', 'SCT', 'Country', 2, true, NOW(), false),
(238, 'Wales', 'WLS', 'Country', 3, true, NOW(), false),
(238, 'Northern Ireland', 'NIR', 'Country', 4, true, NOW(), false);

-- =============================================
-- 🇪🇸 SPAIN (Id: 214) - Autonomous Communities (own parliaments and laws)
-- =============================================
INSERT INTO "States" ("CountryId", "Name", "Code", "Type", "DisplayOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(214, 'Andalusia', 'AN', 'Autonomous community', 1, true, NOW(), false),
(214, 'Aragon', 'AR', 'Autonomous community', 2, true, NOW(), false),
(214, 'Asturias', 'AS', 'Autonomous community', 3, true, NOW(), false),
(214, 'Balearic Islands', 'IB', 'Autonomous community', 4, true, NOW(), false),
(214, 'Basque Country', 'PV', 'Autonomous community', 5, true, NOW(), false),
(214, 'Canary Islands', 'CN', 'Autonomous community', 6, true, NOW(), false),
(214, 'Cantabria', 'CB', 'Autonomous community', 7, true, NOW(), false),
(214, 'Castile and Leon', 'CL', 'Autonomous community', 8, true, NOW(), false),
(214, 'Castile-La Mancha', 'CM', 'Autonomous community', 9, true, NOW(), false),
(214, 'Catalonia', 'CT', 'Autonomous community', 10, true, NOW(), false),
(214, 'Ceuta', 'CE', 'Autonomous city', 11, true, NOW(), false),
(214, 'Extremadura', 'EX', 'Autonomous community', 12, true, NOW(), false),
(214, 'Galicia', 'GA', 'Autonomous community', 13, true, NOW(), false),
(214, 'La Rioja', 'RI', 'Autonomous community', 14, true, NOW(), false),
(214, 'Community of Madrid', 'MD', 'Autonomous community', 15, true, NOW(), false),
(214, 'Melilla', 'ML', 'Autonomous city', 16, true, NOW(), false),
(214, 'Region of Murcia', 'MC', 'Autonomous community', 17, true, NOW(), false),
(214, 'Navarre', 'NC', 'Autonomous community', 18, true, NOW(), false),
(214, 'Valencian Community', 'VC', 'Autonomous community', 19, true, NOW(), false);

-- =============================================
-- 🇺🇦 UKRAINE (Id: 237) - Oblasts (elected oblast councils)
-- =============================================
INSERT INTO "States" ("CountryId", "Name", "Code", "Type", "DisplayOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(237, 'Cherkasy Oblast', '71', 'Oblast', 1, true, NOW(), false),
(237, 'Chernihiv Oblast', '74', 'Oblast', 2, true, NOW(), false),
(237, 'Chernivtsi Oblast', '77', 'Oblast', 3, true, NOW(), false),
(237, 'Crimea', '43', 'Autonomous republic', 4, true, NOW(), false),
(237, 'Dnipropetrovsk Oblast', '12', 'Oblast', 5, true, NOW(), false),
(237, 'Donetsk Oblast', '14', 'Oblast', 6, true, NOW(), false),
(237, 'Ivano-Frankivsk Oblast', '26', 'Oblast', 7, true, NOW(), false),
(237, 'Kharkiv Oblast', '63', 'Oblast', 8, true, NOW(), false),
(237, 'Kherson Oblast', '65', 'Oblast', 9, true, NOW(), false),
(237, 'Khmelnytskyi Oblast', '68', 'Oblast', 10, true, NOW(), false),
(237, 'Kirovohrad Oblast', '35', 'Oblast', 11, true, NOW(), false),
(237, 'Kyiv', '30', 'City', 12, true, NOW(), false),
(237, 'Kyiv Oblast', '32', 'Oblast', 13, true, NOW(), false),
(237, 'Luhansk Oblast', '09', 'Oblast', 14, true, NOW(), false),
(237, 'Lviv Oblast', '46', 'Oblast', 15, true, NOW(), false),
(237, 'Mykolaiv Oblast', '48', 'Oblast', 16, true, NOW(), false),
(237, 'Odessa Oblast', '51', 'Oblast', 17, true, NOW(), false),
(237, 'Poltava Oblast', '53', 'Oblast', 18, true, NOW(), false),
(237, 'Rivne Oblast', '56', 'Oblast', 19, true, NOW(), false),
(237, 'Sevastopol', '40', 'City', 20, true, NOW(), false),
(237, 'Sumy Oblast', '59', 'Oblast', 21, true, NOW(), false),
(237, 'Ternopil Oblast', '61', 'Oblast', 22, true, NOW(), false),
(237, 'Vinnytsia Oblast', '05', 'Oblast', 23, true, NOW(), false),
(237, 'Volyn Oblast', '07', 'Oblast', 24, true, NOW(), false),
(237, 'Zakarpattia Oblast', '21', 'Oblast', 25, true, NOW(), false),
(237, 'Zaporizhzhia Oblast', '23', 'Oblast', 26, true, NOW(), false),
(237, 'Zhytomyr Oblast', '18', 'Oblast', 27, true, NOW(), false);

-- =============================================
-- 🇰🇷 SOUTH KOREA (Id: 212) - Elected governors and local assemblies
-- =============================================
INSERT INTO "States" ("CountryId", "Name", "Code", "Type", "DisplayOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(212, 'Seoul', '11', 'Special city', 1, true, NOW(), false),
(212, 'Busan', '26', 'Metropolitan city', 2, true, NOW(), false),
(212, 'Daegu', '27', 'Metropolitan city', 3, true, NOW(), false),
(212, 'Incheon', '28', 'Metropolitan city', 4, true, NOW(), false),
(212, 'Gwangju', '29', 'Metropolitan city', 5, true, NOW(), false),
(212, 'Daejeon', '30', 'Metropolitan city', 6, true, NOW(), false),
(212, 'Ulsan', '31', 'Metropolitan city', 7, true, NOW(), false),
(212, 'Sejong', '36', 'Special autonomous city', 8, true, NOW(), false),
(212, 'Gyeonggi', '41', 'Province', 9, true, NOW(), false),
(212, 'Gangwon', '42', 'Special self-governing province', 10, true, NOW(), false),
(212, 'North Chungcheong', '43', 'Province', 11, true, NOW(), false),
(212, 'South Chungcheong', '44', 'Province', 12, true, NOW(), false),
(212, 'North Jeolla', '45', 'Province', 13, true, NOW(), false),
(212, 'South Jeolla', '46', 'Province', 14, true, NOW(), false),
(212, 'North Gyeongsang', '47', 'Province', 15, true, NOW(), false),
(212, 'South Gyeongsang', '48', 'Province', 16, true, NOW(), false),
(212, 'Jeju', '49', 'Special self-governing province', 17, true, NOW(), false);

-- =============================================
-- 🇸🇪 SWEDEN (Id: 218) - Counties with elected regional councils
-- =============================================
INSERT INTO "States" ("CountryId", "Name", "Code", "Type", "DisplayOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(218, 'Stockholm', 'AB', 'County', 1, true, NOW(), false),
(218, 'Vasterbotten', 'AC', 'County', 2, true, NOW(), false),
(218, 'Norrbotten', 'BD', 'County', 3, true, NOW(), false),
(218, 'Uppsala', 'C', 'County', 4, true, NOW(), false),
(218, 'Sodermanland', 'D', 'County', 5, true, NOW(), false),
(218, 'Ostergotland', 'E', 'County', 6, true, NOW(), false),
(218, 'Jonkoping', 'F', 'County', 7, true, NOW(), false),
(218, 'Kronoberg', 'G', 'County', 8, true, NOW(), false),
(218, 'Kalmar', 'H', 'County', 9, true, NOW(), false),
(218, 'Gotland', 'I', 'County', 10, true, NOW(), false),
(218, 'Blekinge', 'K', 'County', 11, true, NOW(), false),
(218, 'Skane', 'M', 'County', 12, true, NOW(), false),
(218, 'Halland', 'N', 'County', 13, true, NOW(), false),
(218, 'Vastra Gotaland', 'O', 'County', 14, true, NOW(), false),
(218, 'Varmland', 'S', 'County', 15, true, NOW(), false),
(218, 'Orebro', 'T', 'County', 16, true, NOW(), false),
(218, 'Vastmanland', 'U', 'County', 17, true, NOW(), false),
(218, 'Dalarna', 'W', 'County', 18, true, NOW(), false),
(218, 'Gavleborg', 'X', 'County', 19, true, NOW(), false),
(218, 'Vasternorrland', 'Y', 'County', 20, true, NOW(), false),
(218, 'Jamtland', 'Z', 'County', 21, true, NOW(), false);

-- =============================================
-- 🇺🇾 URUGUAY (Id: 240) - Departments with elected governors (intendentes)
-- =============================================
INSERT INTO "States" ("CountryId", "Name", "Code", "Type", "DisplayOrder", "IsActive", "CreatedAt", "IsDeleted") VALUES
(240, 'Artigas', 'AR', 'Department', 1, true, NOW(), false),
(240, 'Canelones', 'CA', 'Department', 2, true, NOW(), false),
(240, 'Cerro Largo', 'CL', 'Department', 3, true, NOW(), false),
(240, 'Colonia', 'CO', 'Department', 4, true, NOW(), false),
(240, 'Durazno', 'DU', 'Department', 5, true, NOW(), false),
(240, 'Flores', 'FS', 'Department', 6, true, NOW(), false),
(240, 'Florida', 'FD', 'Department', 7, true, NOW(), false),
(240, 'Lavalleja', 'LA', 'Department', 8, true, NOW(), false),
(240, 'Maldonado', 'MA', 'Department', 9, true, NOW(), false),
(240, 'Montevideo', 'MO', 'Department', 10, true, NOW(), false),
(240, 'Paysandu', 'PA', 'Department', 11, true, NOW(), false),
(240, 'Rio Negro', 'RN', 'Department', 12, true, NOW(), false),
(240, 'Rivera', 'RV', 'Department', 13, true, NOW(), false),
(240, 'Rocha', 'RO', 'Department', 14, true, NOW(), false),
(240, 'Salto', 'SA', 'Department', 15, true, NOW(), false),
(240, 'San Jose', 'SJ', 'Department', 16, true, NOW(), false),
(240, 'Soriano', 'SO', 'Department', 17, true, NOW(), false),
(240, 'Tacuarembo', 'TA', 'Department', 18, true, NOW(), false),
(240, 'Treinta y Tres', 'TT', 'Department', 19, true, NOW(), false);

COMMIT;
