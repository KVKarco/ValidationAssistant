using KVKarco.ValidationAssistant.Abstractions.MessageTemplate;
using KVKarco.ValidationAssistant.Internal.Utilities.MessageTemplates;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KVKarco.ValidationAssistant;

public static class ValidationAssistantConfig
{
    public static readonly DefaultConfiguration GlobalDefaults = new();
}
public static class TemplatesNames
{
    public const string DefaultFailure = "Generic_Failure";
    public const string NotNull = "Not_Null";
    public const string MustBeNull = "Must_Be_Null";
    public const string ChildValidator = "Child_Validator";
    public const string ConditionalFlowStop = "Conditional_Flow_Stop";
}

public static class Placeholders
{
    public const string PropertyName = "PropertyName";
}

public sealed class DefaultConfiguration
{
    public DefaultConfiguration()
    {
        MessageTemplates = new MessageTemplateRegistrar();
        MessageTemplates.RegisterFrom(new BuildInTemplates());
    }

    public CultureInfo DefaultCulture { get; set; } = CultureInfo.GetCultureInfo("en-US");

    public IMessageTemplateRegistrar MessageTemplates { get; }

    public FlowEffect DefaultValidatorLevelFlowEffect { get; set; } = FlowEffect.Proceed;

    public FlowEffect DefaultRuleLevelFlowEffect { get; set; } = FlowEffect.Proceed;

    public Severity DefaultRuleFailureSeverity { get; set; } = Severity.Error;

    public bool UseExceptionsOnValidation { get; set; }

    public bool UseExceptionsOnPreValidation { get; set; }
}

public class BuildInTemplates : IMessageTemplateSource
{
    public void Register([NotNull] IMessageTemplateRegistrar registrar)
    {
        // English (en-US)
        CultureInfo enUSCulture = CultureInfo.GetCultureInfo("en-US");
        registrar.Register(TemplatesNames.DefaultFailure, enUSCulture, "The specified condition was not met for '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, enUSCulture, "'{PropertyName}' must not be empty.");
        registrar.Register(TemplatesNames.MustBeNull, enUSCulture, "'{PropertyName}' must be empty.");
        registrar.Register(TemplatesNames.ChildValidator, enUSCulture, "Nested validator for '{PropertyName}' encounter failures.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, enUSCulture, "Condition or snapshot stop the execution of the remaining rules in the current RuleSet.");

        // Spanish (es-ES)
        CultureInfo esESCulture = CultureInfo.GetCultureInfo("es-ES");
        registrar.Register(TemplatesNames.DefaultFailure, esESCulture, "La condición especificada no se cumplió para '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, esESCulture, "'{PropertyName}' no debe estar vacío.");
        registrar.Register(TemplatesNames.MustBeNull, esESCulture, "'{PropertyName}' debe estar vacío.");
        registrar.Register(TemplatesNames.ChildValidator, esESCulture, "El validador anidado para '{PropertyName}' encontró errores.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, esESCulture, "La condición o la instantánea detuvieron la ejecución de las reglas restantes en el RuleSet actual.");

        // Chinese (Simplified) (zh-CN)
        CultureInfo zhCNCulture = CultureInfo.GetCultureInfo("zh-CN");
        registrar.Register(TemplatesNames.DefaultFailure, zhCNCulture, "为 '{PropertyName}' 指定的条件未满足。");
        registrar.Register(TemplatesNames.NotNull, zhCNCulture, "'{PropertyName}' 不能为空。");
        registrar.Register(TemplatesNames.MustBeNull, zhCNCulture, "'{PropertyName}' 必须为空。");
        registrar.Register(TemplatesNames.ChildValidator, zhCNCulture, "为 '{PropertyName}' 的嵌套验证器遇到失败。");
        registrar.Register(TemplatesNames.ConditionalFlowStop, zhCNCulture, "条件或快照停止了当前 RuleSet 中剩余规则的执行。");

        // Hindi (hi-IN)
        CultureInfo hiINCulture = CultureInfo.GetCultureInfo("hi-IN");
        registrar.Register(TemplatesNames.DefaultFailure, hiINCulture, "'{PropertyName}' के लिए निर्दिष्ट शर्त पूरी नहीं हुई।");
        registrar.Register(TemplatesNames.NotNull, hiINCulture, "'{PropertyName}' खाली नहीं होना चाहिए।");
        registrar.Register(TemplatesNames.MustBeNull, hiINCulture, "'{PropertyName}' खाली होना चाहिए।");
        registrar.Register(TemplatesNames.ChildValidator, hiINCulture, "'{PropertyName}' के लिए नेस्टेड वैलिडेटर को विफलता मिली।");
        registrar.Register(TemplatesNames.ConditionalFlowStop, hiINCulture, "शर्त या स्नैपशॉट ने मौजूदा RuleSet में बाकी नियमों का निष्पादन रोक दिया।");

        // Portuguese (pt-BR)
        CultureInfo ptBRCulture = CultureInfo.GetCultureInfo("pt-BR");
        registrar.Register(TemplatesNames.DefaultFailure, ptBRCulture, "A condição especificada não foi atendida para '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ptBRCulture, "'{PropertyName}' não deve estar vazio.");
        registrar.Register(TemplatesNames.MustBeNull, ptBRCulture, "'{PropertyName}' deve estar vazio.");
        registrar.Register(TemplatesNames.ChildValidator, ptBRCulture, "O validador aninhado para '{PropertyName}' encontrou falhas.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ptBRCulture, "A condição ou instantâneo interrompeu a execução das regras restantes no RuleSet atual.");

        // Bengali (bn-BD)
        CultureInfo bnBDCulture = CultureInfo.GetCultureInfo("bn-BD");
        registrar.Register(TemplatesNames.DefaultFailure, bnBDCulture, "'{PropertyName}' এর জন্য নির্দিষ্ট শর্ত পূরণ হয়নি।");
        registrar.Register(TemplatesNames.NotNull, bnBDCulture, "'{PropertyName}' খালি হওয়া যাবে না।");
        registrar.Register(TemplatesNames.MustBeNull, bnBDCulture, "'{PropertyName}' খালি হতে হবে।");
        registrar.Register(TemplatesNames.ChildValidator, bnBDCulture, "'{PropertyName}' এর জন্য নেস্টেড ভ্যালিডেটর ব্যর্থতা খুঁজে পেয়েছে।");
        registrar.Register(TemplatesNames.ConditionalFlowStop, bnBDCulture, "শর্ত বা স্ন্যাপশট বর্তমান RuleSet-এর বাকি নিয়মগুলোর কার্যকারিতা বন্ধ করে দিয়েছে।");

        // Russian (ru-RU)
        CultureInfo ruRUCulture = CultureInfo.GetCultureInfo("ru-RU");
        registrar.Register(TemplatesNames.DefaultFailure, ruRUCulture, "Указанное условие не было выполнено для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ruRUCulture, "'{PropertyName}' не должен быть пустым.");
        registrar.Register(TemplatesNames.MustBeNull, ruRUCulture, "'{PropertyName}' должен быть пустым.");
        registrar.Register(TemplatesNames.ChildValidator, ruRUCulture, "Вложенный валидатор для '{PropertyName}' обнаружил ошибки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ruRUCulture, "Условие или моментальный снимок останавливают выполнение остальных правил в текущем RuleSet.");

        // Japanese (ja-JP)
        CultureInfo jaJPCulture = CultureInfo.GetCultureInfo("ja-JP");
        registrar.Register(TemplatesNames.DefaultFailure, jaJPCulture, "'{PropertyName}' に指定された条件が満たされませんでした。");
        registrar.Register(TemplatesNames.NotNull, jaJPCulture, "'{PropertyName}' は空であってはなりません。");
        registrar.Register(TemplatesNames.MustBeNull, jaJPCulture, "'{PropertyName}' は空でなければなりません。");
        registrar.Register(TemplatesNames.ChildValidator, jaJPCulture, "'{PropertyName}' のネストされたバリデーターが失敗を検出しました。");
        registrar.Register(TemplatesNames.ConditionalFlowStop, jaJPCulture, "条件またはスナップショットにより、現在の RuleSet の残りのルールの実行が停止されました。");

        // German (de-DE)
        CultureInfo deDECulture = CultureInfo.GetCultureInfo("de-DE");
        registrar.Register(TemplatesNames.DefaultFailure, deDECulture, "Die angegebene Bedingung wurde für '{PropertyName}' nicht erfüllt.");
        registrar.Register(TemplatesNames.NotNull, deDECulture, "'{PropertyName}' darf nicht leer sein.");
        registrar.Register(TemplatesNames.MustBeNull, deDECulture, "'{PropertyName}' muss leer sein.");
        registrar.Register(TemplatesNames.ChildValidator, deDECulture, "Verschachtelter Validator für '{PropertyName}' hat Fehler.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, deDECulture, "Bedingung oder Schnappschuss stoppt die Ausführung der restlichen Regeln im aktuellen Regelwerk (RuleSet).");

        // French (fr-FR)
        CultureInfo frFRCulture = CultureInfo.GetCultureInfo("fr-FR");
        registrar.Register(TemplatesNames.DefaultFailure, frFRCulture, "La condition spécifiée n'a pas été remplie pour '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, frFRCulture, "'{PropertyName}' ne doit pas être vide.");
        registrar.Register(TemplatesNames.MustBeNull, frFRCulture, "'{PropertyName}' doit être vide.");
        registrar.Register(TemplatesNames.ChildValidator, frFRCulture, "Le validateur imbriqué pour '{PropertyName}' a rencontré des échecs.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, frFRCulture, "La condition ou l'instantané arrête l'exécution des règles restantes dans le RuleSet actuel.");

        // Italian (it-IT)
        CultureInfo itITCulture = CultureInfo.GetCultureInfo("it-IT");
        registrar.Register(TemplatesNames.DefaultFailure, itITCulture, "La condizione specificata non è stata soddisfatta per '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, itITCulture, "'{PropertyName}' non deve essere vuoto.");
        registrar.Register(TemplatesNames.MustBeNull, itITCulture, "'{PropertyName}' deve essere vuoto.");
        registrar.Register(TemplatesNames.ChildValidator, itITCulture, "Il validatore annidato per '{PropertyName}' ha riscontrato errori.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, itITCulture, "La condizione o lo snapshot bloccano l'esecuzione delle regole rimanenti nel RuleSet corrente.");

        // Korean (ko-KR)
        CultureInfo koKRCulture = CultureInfo.GetCultureInfo("ko-KR");
        registrar.Register(TemplatesNames.DefaultFailure, koKRCulture, "'{PropertyName}'에 대해 지정된 조건이 충족되지 않았습니다.");
        registrar.Register(TemplatesNames.NotNull, koKRCulture, "'{PropertyName}'은 비워둘 수 없습니다.");
        registrar.Register(TemplatesNames.MustBeNull, koKRCulture, "'{PropertyName}'은 비어 있어야 합니다.");
        registrar.Register(TemplatesNames.ChildValidator, koKRCulture, "'{PropertyName}'에 대한 중첩된 유효성 검사기에서 실패가 발생했습니다.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, koKRCulture, "조건 또는 스냅샷으로 인해 현재 RuleSet의 나머지 규칙 실행이 중지되었습니다.");

        // Turkish (tr-TR)
        CultureInfo trTRCulture = CultureInfo.GetCultureInfo("tr-TR");
        registrar.Register(TemplatesNames.DefaultFailure, trTRCulture, "'{PropertyName}' için belirtilen koşul karşılanmadı.");
        registrar.Register(TemplatesNames.NotNull, trTRCulture, "'{PropertyName}' boş olmamalıdır.");
        registrar.Register(TemplatesNames.MustBeNull, trTRCulture, "'{PropertyName}' boş olmalıdır.");
        registrar.Register(TemplatesNames.ChildValidator, trTRCulture, "'{PropertyName}' için iç içe doğrulayıcıda hatalar oluştu.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, trTRCulture, "Koşul veya anlık görüntü, mevcut RuleSet'teki geri kalan kuralların yürütülmesini durdurur.");

        // Vietnamese (vi-VN)
        CultureInfo viVNCulture = CultureInfo.GetCultureInfo("vi-VN");
        registrar.Register(TemplatesNames.DefaultFailure, viVNCulture, "Điều kiện được chỉ định không được đáp ứng cho '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, viVNCulture, "'{PropertyName}' không được để trống.");
        registrar.Register(TemplatesNames.MustBeNull, viVNCulture, "'{PropertyName}' phải để trống.");
        registrar.Register(TemplatesNames.ChildValidator, viVNCulture, "Trình xác thực lồng nhau cho '{PropertyName}' gặp phải lỗi.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, viVNCulture, "Điều kiện hoặc ảnh chụp nhanh đã dừng việc thực thi các quy tắc còn lại trong RuleSet hiện tại.");

        // Dutch (nl-NL)
        CultureInfo nlNLCulture = CultureInfo.GetCultureInfo("nl-NL");
        registrar.Register(TemplatesNames.DefaultFailure, nlNLCulture, "De opgegeven voorwaarde werd niet voldaan voor '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, nlNLCulture, "'{PropertyName}' mag niet leeg zijn.");
        registrar.Register(TemplatesNames.MustBeNull, nlNLCulture, "'{PropertyName}' moet leeg zijn.");
        registrar.Register(TemplatesNames.ChildValidator, nlNLCulture, "Geneste validator voor '{PropertyName}' heeft fouten aangetroffen.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, nlNLCulture, "Voorwaarde of momentopname stopt de uitvoering van de resterende regels in de huidige RuleSet.");

        // Thai (th-TH)
        CultureInfo thTHCulture = CultureInfo.GetCultureInfo("th-TH");
        registrar.Register(TemplatesNames.DefaultFailure, thTHCulture, "เงื่อนไขที่ระบุไม่ตรงตามข้อกำหนดสำหรับ '{PropertyName}'");
        registrar.Register(TemplatesNames.NotNull, thTHCulture, "'{PropertyName}' ต้องไม่ว่างเปล่า");
        registrar.Register(TemplatesNames.MustBeNull, thTHCulture, "'{PropertyName}' ต้องว่างเปล่า");
        registrar.Register(TemplatesNames.ChildValidator, thTHCulture, "ตัวตรวจสอบที่ซ้อนกันสำหรับ '{PropertyName}' พบข้อผิดพลาด");
        registrar.Register(TemplatesNames.ConditionalFlowStop, thTHCulture, "เงื่อนไขหรือสแนปช็อตหยุดการทำงานของกฎที่เหลือใน RuleSet ปัจจุบัน");

        // Polish (pl-PL)
        CultureInfo plPLCulture = CultureInfo.GetCultureInfo("pl-PL");
        registrar.Register(TemplatesNames.DefaultFailure, plPLCulture, "Określony warunek nie został spełniony dla '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, plPLCulture, "'{PropertyName}' nie może być pusty.");
        registrar.Register(TemplatesNames.MustBeNull, plPLCulture, "'{PropertyName}' musi być pusty.");
        registrar.Register(TemplatesNames.ChildValidator, plPLCulture, "Zagnieżdżony walidator dla '{PropertyName}' napotkał błędy.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, plPLCulture, "Warunek lub migawka zatrzymuje wykonanie pozostałych reguł w bieżącym RuleSet.");

        // Ukrainian (uk-UA)
        CultureInfo ukUACulture = CultureInfo.GetCultureInfo("uk-UA");
        registrar.Register(TemplatesNames.DefaultFailure, ukUACulture, "Задана умова не була виконана для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ukUACulture, "'{PropertyName}' не повинно бути порожнім.");
        registrar.Register(TemplatesNames.MustBeNull, ukUACulture, "'{PropertyName}' має бути порожнім.");
        registrar.Register(TemplatesNames.ChildValidator, ukUACulture, "Вкладений валідатор для '{PropertyName}' виявив помилки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ukUACulture, "Умова або знімок зупиняють виконання решти правил у поточному RuleSet.");

        // Arabic (ar-SA)
        CultureInfo arSACulture = CultureInfo.GetCultureInfo("ar-SA");
        registrar.Register(TemplatesNames.DefaultFailure, arSACulture, "الشرط المحدد لم يتم الوفاء به لـ '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, arSACulture, "يجب ألا يكون '{PropertyName}' فارغًا.");
        registrar.Register(TemplatesNames.MustBeNull, arSACulture, "يجب أن يكون '{PropertyName}' فارغًا.");
        registrar.Register(TemplatesNames.ChildValidator, arSACulture, "حدثت إخفاقات في المدقق المتداخل لـ '{PropertyName}'.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, arSACulture, "الشرط أو اللقطة يوقفان تنفيذ القواعد المتبقية في RuleSet الحالي.");

        // Farsi (fa-IR)
        CultureInfo faIRCulture = CultureInfo.GetCultureInfo("fa-IR");
        registrar.Register(TemplatesNames.DefaultFailure, faIRCulture, "شرط تعیین شده برای '{PropertyName}' برآورده نشد.");
        registrar.Register(TemplatesNames.NotNull, faIRCulture, "'{PropertyName}' نباید خالی باشد.");
        registrar.Register(TemplatesNames.MustBeNull, faIRCulture, "'{PropertyName}' باید خالی باشد.");
        registrar.Register(TemplatesNames.ChildValidator, faIRCulture, "اعتبارسنجی تودرتو برای '{PropertyName}' با شکست مواجه شد.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, faIRCulture, "شرط یا اسنپ شات اجرای قوانین باقی‌مانده در RuleSet فعلی را متوقف کرد.");

        // Urdu (ur-PK)
        CultureInfo urPKCulture = CultureInfo.GetCultureInfo("ur-PK");
        registrar.Register(TemplatesNames.DefaultFailure, urPKCulture, "مخصوص شرط '{PropertyName}' کے لیے پوری نہیں ہوئی۔");
        registrar.Register(TemplatesNames.NotNull, urPKCulture, "'{PropertyName}' خالی نہیں ہونا چاہیے۔");
        registrar.Register(TemplatesNames.MustBeNull, urPKCulture, "'{PropertyName}' خالی ہونا چاہیے۔");
        registrar.Register(TemplatesNames.ChildValidator, urPKCulture, "'{PropertyName}' کے لیے نیسٹڈ ویلیڈیٹر کو ناکامیاں پیش آئیں۔");
        registrar.Register(TemplatesNames.ConditionalFlowStop, urPKCulture, "حالت یا اسنیپ شاٹ موجودہ RuleSet میں باقی قواعد کی عمل درآمد روکتے ہیں۔");

        // Indonesian (id-ID)
        CultureInfo idIDCulture = CultureInfo.GetCultureInfo("id-ID");
        registrar.Register(TemplatesNames.DefaultFailure, idIDCulture, "Kondisi yang ditentukan tidak terpenuhi untuk '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, idIDCulture, "'{PropertyName}' tidak boleh kosong.");
        registrar.Register(TemplatesNames.MustBeNull, idIDCulture, "'{PropertyName}' harus kosong.");
        registrar.Register(TemplatesNames.ChildValidator, idIDCulture, "Validator bersarang untuk '{PropertyName}' mengalami kegagalan.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, idIDCulture, "Kondisi atau snapshot menghentikan eksekusi aturan yang tersisa di RuleSet saat ini.");

        // Swahili (sw-KE)
        CultureInfo swKECulture = CultureInfo.GetCultureInfo("sw-KE");
        registrar.Register(TemplatesNames.DefaultFailure, swKECulture, "Masharti yaliyotajwa hayakutimizwa kwa '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, swKECulture, "'{PropertyName}' haipaswi kuwa tupu.");
        registrar.Register(TemplatesNames.MustBeNull, swKECulture, "'{PropertyName}' lazima iwe tupu.");
        registrar.Register(TemplatesNames.ChildValidator, swKECulture, "Kithibitishaji kilichowekwa ndani kwa '{PropertyName}' kimepata hitilafu.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, swKECulture, "Hali au picha inasitisha utekelezaji wa sheria zilizosalia katika RuleSet ya sasa.");

        // Javanese (jv-ID)
        CultureInfo jvIDCulture = CultureInfo.GetCultureInfo("jv-ID");
        registrar.Register(TemplatesNames.DefaultFailure, jvIDCulture, "Kahanan sing ditemtokake ora ketemu kanggo '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, jvIDCulture, "'{PropertyName}' ora kena kosong.");
        registrar.Register(TemplatesNames.MustBeNull, jvIDCulture, "'{PropertyName}' kudu kosong.");
        registrar.Register(TemplatesNames.ChildValidator, jvIDCulture, "Validator bersarang kanggo '{PropertyName}' nemoni kegagalan.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, jvIDCulture, "Kahanan utawa snapshot nyetop eksekusi aturan sing isih ana ing RuleSet saiki.");

        // Punjabi (pa-IN)
        CultureInfo paINCulture = CultureInfo.GetCultureInfo("pa-IN");
        registrar.Register(TemplatesNames.DefaultFailure, paINCulture, "'{PropertyName}' ਲਈ ਨਿਰਧਾਰਤ ਸ਼ਰਤ ਪੂਰੀ ਨਹੀਂ ਹੋਈ।");
        registrar.Register(TemplatesNames.NotNull, paINCulture, "'{PropertyName}' ਖਾਲੀ ਨਹੀਂ ਹੋਣਾ ਚਾਹੀਦਾ ਹੈ।");
        registrar.Register(TemplatesNames.MustBeNull, paINCulture, "'{PropertyName}' ਖਾਲੀ ਹੋਣਾ ਚਾਹੀਦਾ ਹੈ।");
        registrar.Register(TemplatesNames.ChildValidator, paINCulture, "'{PropertyName}' ਲਈ ਨੈਸਟਡ ਵੈਲੀਡੇਟਰ ਨੂੰ ਅਸਫਲਤਾਵਾਂ ਮਿਲੀਆਂ।");
        registrar.Register(TemplatesNames.ConditionalFlowStop, paINCulture, "ਸ਼ਰਤ ਜਾਂ ਸਨੈਪਸ਼ਾਟ ਮੌਜੂਦਾ RuleSet ਵਿੱਚ ਬਾਕੀ ਬਚੇ ਨਿਯਮਾਂ ਦੀ ਕਾਰਵਾਈ ਨੂੰ ਰੋਕਦੇ ਹਨ।");

        // Telugu (te-IN)
        CultureInfo teINCulture = CultureInfo.GetCultureInfo("te-IN");
        registrar.Register(TemplatesNames.DefaultFailure, teINCulture, "'{PropertyName}' కొరకు నిర్దిష్ట షరతు నెరవేరలేదు.");
        registrar.Register(TemplatesNames.NotNull, teINCulture, "'{PropertyName}' ఖాళీగా ఉండకూడదు.");
        registrar.Register(TemplatesNames.MustBeNull, teINCulture, "'{PropertyName}' ఖాళీగా ఉండాలి.");
        registrar.Register(TemplatesNames.ChildValidator, teINCulture, "'{PropertyName}' కోసం గూడుకట్టిన ధ్రువీకరణకర్త వైఫల్యాలను ఎదుర్కొంది.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, teINCulture, "నిబంధన లేదా స్నాప్‌షాట్ ప్రస్తుత RuleSetలోని మిగిలిన నియమాల అమలును నిలిపివేసింది.");

        // Marathi (mr-IN)
        CultureInfo mrINCulture = CultureInfo.GetCultureInfo("mr-IN");
        registrar.Register(TemplatesNames.DefaultFailure, mrINCulture, "'{PropertyName}' साठी निर्दिष्ट केलेली अट पूर्ण झाली नाही.");
        registrar.Register(TemplatesNames.NotNull, mrINCulture, "'{PropertyName}' रिक्त नसावे.");
        registrar.Register(TemplatesNames.MustBeNull, mrINCulture, "'{PropertyName}' रिक्त असले पाहिजे.");
        registrar.Register(TemplatesNames.ChildValidator, mrINCulture, "'{PropertyName}' साठी नेस्टेड व्हॅलिडेटरमध्ये त्रुटी आढळल्या.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, mrINCulture, "अट किंवा स्नॅपशॉट चालू RuleSet मधील उर्वरित नियमांचे अंमलबजावणी थांबवते.");

        // Tamil (ta-IN)
        CultureInfo taINCulture = CultureInfo.GetCultureInfo("ta-IN");
        registrar.Register(TemplatesNames.DefaultFailure, taINCulture, "'{PropertyName}' க்கு குறிப்பிட்ட நிபந்தனை பூர்த்தி செய்யப்படவில்லை.");
        registrar.Register(TemplatesNames.NotNull, taINCulture, "'{PropertyName}' காலியாக இருக்கக்கூடாது.");
        registrar.Register(TemplatesNames.MustBeNull, taINCulture, "'{PropertyName}' காலியாக இருக்க வேண்டும்.");
        registrar.Register(TemplatesNames.ChildValidator, taINCulture, "'{PropertyName}' க்கான உள்ளிடப்பட்ட சரிபார்ப்பு தோல்விகளை சந்தித்தது.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, taINCulture, "நிபந்தனை அல்லது ஸ்னாப்ஷாட் தற்போதைய RuleSet இல் உள்ள மீதமுள்ள விதிகளின் செயல்பாட்டை நிறுத்துகிறது.");

        // Malay (ms-MY)
        CultureInfo msMYCulture = CultureInfo.GetCultureInfo("ms-MY");
        registrar.Register(TemplatesNames.DefaultFailure, msMYCulture, "Syarat yang ditetapkan tidak dipenuhi untuk '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, msMYCulture, "'{PropertyName}' tidak boleh kosong.");
        registrar.Register(TemplatesNames.MustBeNull, msMYCulture, "'{PropertyName}' mesti kosong.");
        registrar.Register(TemplatesNames.ChildValidator, msMYCulture, "Pengesah bersarang untuk '{PropertyName}' menemui kegagalan.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, msMYCulture, "Syarat atau snapshot menghentikan pelaksanaan peraturan yang tinggal dalam RuleSet semasa.");

        // Yoruba (yo-NG)
        CultureInfo yoNGCulture = CultureInfo.GetCultureInfo("yo-NG");
        registrar.Register(TemplatesNames.DefaultFailure, yoNGCulture, "Àìtẹ̀lé àṣẹ tí a fìdí rẹ̀ múlẹ̀ fún '{PropertyName}' kò ṣẹlẹ̀.");
        registrar.Register(TemplatesNames.NotNull, yoNGCulture, "'{PropertyName}' kò gbọ́dọ̀ jẹ́ òfo.");
        registrar.Register(TemplatesNames.MustBeNull, yoNGCulture, "'{PropertyName}' gbọ́dọ̀ jẹ́ òfo.");
        registrar.Register(TemplatesNames.ChildValidator, yoNGCulture, "Ìṣèyẹ̀wò àìtẹ̀lé àṣẹ tí a fi sínú '{PropertyName}' ti dojú kọ àwọn àìṣẹlẹ̀.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, yoNGCulture, "Àṣẹ tàbí àwòrán ti dádúró ìṣiṣẹ́ àwọn òfin tí ó kù nínú RuleSet ìsinsìnyí.");

        // Hausa (ha-NG)
        CultureInfo haNGCulture = CultureInfo.GetCultureInfo("ha-NG");
        registrar.Register(TemplatesNames.DefaultFailure, haNGCulture, "Sharuɗɗan da aka ƙayyade ba a cika su ba don '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, haNGCulture, "'{PropertyName}' dole ne kada ya zama fanko.");
        registrar.Register(TemplatesNames.MustBeNull, haNGCulture, "'{PropertyName}' dole ne ya zama fanko.");
        registrar.Register(TemplatesNames.ChildValidator, haNGCulture, "Mai binciken da aka gina a ciki don '{PropertyName}' ya haɗu da gazawa.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, haNGCulture, "Sharuɗɗa ko hoto sun dakatar da aiwatar da sauran dokoki a cikin RuleSet na yanzu.");

        // Swahili (sw-TZ)
        CultureInfo swTZCulture = CultureInfo.GetCultureInfo("sw-TZ");
        registrar.Register(TemplatesNames.DefaultFailure, swTZCulture, "Masharti yaliyotajwa hayakutimizwa kwa '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, swTZCulture, "'{PropertyName}' haipaswi kuwa tupu.");
        registrar.Register(TemplatesNames.MustBeNull, swTZCulture, "'{PropertyName}' lazima iwe tupu.");
        registrar.Register(TemplatesNames.ChildValidator, swTZCulture, "Kithibitishaji kilichowekwa ndani kwa '{PropertyName}' kimepata hitilafu.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, swTZCulture, "Hali au picha inasitisha utekelezaji wa sheria zilizosalia katika RuleSet ya sasa.");

        // Somali (so-SO)
        CultureInfo soSOCulture = CultureInfo.GetCultureInfo("so-SO");
        registrar.Register(TemplatesNames.DefaultFailure, soSOCulture, "Shardi la cayiman lama buuxin '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, soSOCulture, "'{PropertyName}' waa inuusan noqon mid faaruq ah.");
        registrar.Register(TemplatesNames.MustBeNull, soSOCulture, "'{PropertyName}' waa inuu noqdo mid faaruq ah.");
        registrar.Register(TemplatesNames.ChildValidator, soSOCulture, "Baaraha ku dhex jira '{PropertyName}' wuxuu la kulmay guul-darrooyin.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, soSOCulture, "Shardi ama sawir-qaadistu waxay joojiyaan fulinta xeerarka harsan ee RuleSet-ka hadda.");

        // Amharic (am-ET)
        CultureInfo amETCulture = CultureInfo.GetCultureInfo("am-ET");
        registrar.Register(TemplatesNames.DefaultFailure, amETCulture, "'{PropertyName}' ን ለመወሰን የተሰጠው መስፈርት አልተሟላም።");
        registrar.Register(TemplatesNames.NotNull, amETCulture, "'{PropertyName}' ባዶ መሆን የለበትም።");
        registrar.Register(TemplatesNames.MustBeNull, amETCulture, "'{PropertyName}' ባዶ መሆን አለበት።");
        registrar.Register(TemplatesNames.ChildValidator, amETCulture, "'{PropertyName}' ውስጥ የተዋቀረ ማረጋገጫ ከስህተቶች ጋር ተጋፍቷል።");
        registrar.Register(TemplatesNames.ConditionalFlowStop, amETCulture, "መስፈርት ወይም ቅጽበታዊ ፎቶው በአሁኑ RuleSet ውስጥ ያሉትን የቀሩትን ህጎች አፈጻጸም አቁመዋል።");

        // Oromo (om-ET)
        CultureInfo omETCulture = CultureInfo.GetCultureInfo("om-ET");
        registrar.Register(TemplatesNames.DefaultFailure, omETCulture, "Haalli murtaa'e '{PropertyName}' irratti hin guutamne.");
        registrar.Register(TemplatesNames.NotNull, omETCulture, "'{PropertyName}' duwwaa hin ta'in.");
        registrar.Register(TemplatesNames.MustBeNull, omETCulture, "'{PropertyName}' duwwaa ta'uu qaba.");
        registrar.Register(TemplatesNames.ChildValidator, omETCulture, "Waan mirkaneessituu walitti hidhame '{PropertyName}' irratti kufaatii argateera.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, omETCulture, "Haalli ykn suuraan hojiirra oolmaa seeraalee hafan RuleSet ammaa keessaa dhaabeera.");

        // Zula (zu-ZA)
        CultureInfo zuZACulture = CultureInfo.GetCultureInfo("zu-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, zuZACulture, "Isimo esishiwo asihlangabezwanga ku '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, zuZACulture, "'{PropertyName}' akumele ibe ngenalutho.");
        registrar.Register(TemplatesNames.MustBeNull, zuZACulture, "'{PropertyName}' kumele ibe ngenalutho.");
        registrar.Register(TemplatesNames.ChildValidator, zuZACulture, "Isiqinisekisi esifakwe ngaphakathi ku '{PropertyName}' sihlangabezana nokwehluleka.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, zuZACulture, "Isimo noma isithombe somhlaba siyamisa ukusebenza kwezinye izinqubo ku RuleSet yamanje.");

        // Xhosa (xh-ZA)
        CultureInfo xhZACulture = CultureInfo.GetCultureInfo("xh-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, xhZACulture, "Imo echaziweyo ayizange ifezeke kwi '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, xhZACulture, "'{PropertyName}' mayingabi nalutho.");
        registrar.Register(TemplatesNames.MustBeNull, xhZACulture, "'{PropertyName}' mayibe nalutho.");
        registrar.Register(TemplatesNames.ChildValidator, xhZACulture, "Umgunyazisi osembindini we '{PropertyName}' uhlangabezane nokungaphumeleli.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, xhZACulture, "Imo okanye isnapshoti imisa ukwenziwa kwemithetho eseleyo kwi RuleSet yangoku.");

        // Swati (ss-ZA)
        CultureInfo ssZACulture = CultureInfo.GetCultureInfo("ss-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, ssZACulture, "Simo lesibekiwe asizange sifezeke ku '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ssZACulture, "'{PropertyName}' akumele kube lishumi.");
        registrar.Register(TemplatesNames.MustBeNull, ssZACulture, "'{PropertyName}' kufanele kube lishumi.");
        registrar.Register(TemplatesNames.ChildValidator, ssZACulture, "Ligciso lelizingozi le '{PropertyName}' laba nekwehluleka.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ssZACulture, "Simo noma isithombe somhlaba siyamisa kusebentisa kwemitsetfo lesele ku RuleSet yamanje.");

        // Sepedi (nso-ZA)
        CultureInfo nsoZACulture = CultureInfo.GetCultureInfo("nso-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, nsoZACulture, "Boemo bjo bo beilwego ga bo a fihlelelwa go '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, nsoZACulture, "'{PropertyName}' ga e swanelwe ke go ba sešilo.");
        registrar.Register(TemplatesNames.MustBeNull, nsoZACulture, "'{PropertyName}' e swanetše go ba sešilo.");
        registrar.Register(TemplatesNames.ChildValidator, nsoZACulture, "Mokgathatšego o o lego ka gare ga '{PropertyName}' o kopane le go se atlege.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, nsoZACulture, "Boemo bja tšhadišano bo kgona go emiša go šoma ga melao ye e šetšego go RuleSet ya bjale.");

        // Tsonga (ts-ZA)
        CultureInfo tsZACulture = CultureInfo.GetCultureInfo("ts-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, tsZACulture, "Xiyimo lexi hlamuselweke a xi fikelelwa hi '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, tsZACulture, "'{PropertyName}' a hi fanela ku va xilo. ");
        registrar.Register(TemplatesNames.MustBeNull, tsZACulture, "'{PropertyName}' i fanela ku va xilo.");
        registrar.Register(TemplatesNames.ChildValidator, tsZACulture, "Ndzivisi wa vundhawu wa '{PropertyName}' a a kuma ku tsandzeka.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, tsZACulture, "Xiyimo kumbe mufananiso wa ntsweti wu yimisa ku endliwa ka milawu leyi sala ka RuleSet ya sweswi.");

        // Sotho (st-ZA)
        CultureInfo stZACulture = CultureInfo.GetCultureInfo("st-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, stZACulture, "Boemo bo hlalositsoeng ha bo a finyelleha ho '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, stZACulture, "'{PropertyName}' ha e sa tlameha ho ba letho.");
        registrar.Register(TemplatesNames.MustBeNull, stZACulture, "'{PropertyName}' e tlameha ho ba letho.");
        registrar.Register(TemplatesNames.ChildValidator, stZACulture, "Mokgathatšiso o kenngwe ho '{PropertyName}' o kopane le ho se atlehe.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, stZACulture, "Boemo kapa snap shot e emisa ho sebetsa ha melao e setseng ho RuleSet ya hajwale.");

        // Venda (ve-ZA)
        CultureInfo veZACulture = CultureInfo.GetCultureInfo("ve-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, veZACulture, "Vhuimo vhune ha vha u '{PropertyName}' a vhusi fhulufhedzwa.");
        registrar.Register(TemplatesNames.NotNull, veZACulture, "'{PropertyName}' a vhu nga si vhe tshihuluhulu.");
        registrar.Register(TemplatesNames.MustBeNull, veZACulture, "'{PropertyName}' vhu nga vha tshihuluhulu.");
        registrar.Register(TemplatesNames.ChildValidator, veZACulture, "Tshishandisi tsha vhune ha '{PropertyName}' a tshi vha tshi vhusi na thukiso.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, veZACulture, "Vhuimo kana 'snapshot' tshi nga emisa 'RuleSet' i dzivha kha vhune ha nga si vhe vhukati ha vhune ha nga si vhe.");

        // Ndebele (nr-ZA)
        CultureInfo nrZACulture = CultureInfo.GetCultureInfo("nr-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, nrZACulture, "Isimo esishiwo asifikelelwe ku '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, nrZACulture, "'{PropertyName}' akumele kube nalutho.");
        registrar.Register(TemplatesNames.MustBeNull, nrZACulture, "'{PropertyName}' kumele kube nalutho.");
        registrar.Register(TemplatesNames.ChildValidator, nrZACulture, "Umgunyazisi ofakwe phakathi kwe '{PropertyName}' uhlangabezana nokungaphumeleli.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, nrZACulture, "Isimo noma isithombe somhlaba siyamisa ukusebenza kwezinye izinqubo ku RuleSet yamanje.");

        // Shona (sn-ZA)
        CultureInfo snZACulture = CultureInfo.GetCultureInfo("sn-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, snZACulture, "Chimiro chakatsanangurwa hachina kusangana ne '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, snZACulture, "'{PropertyName}' haifanire kunge isina chinhu.");
        registrar.Register(TemplatesNames.MustBeNull, snZACulture, "'{PropertyName}' inofanira kunge isina chinhu.");
        registrar.Register(TemplatesNames.ChildValidator, snZACulture, "Chinyorwa che '{PropertyName}' chine kusakundika.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, snZACulture, "Chimiro kana 'snapshot' inokwanisa kumisa kuitwa kwemitemo iri pamberi pe '{PropertyName}'.");

        // Afrikaans (af-ZA)
        CultureInfo afZACulture = CultureInfo.GetCultureInfo("af-ZA");
        registrar.Register(TemplatesNames.DefaultFailure, afZACulture, "Die gespesifiseerde voorwaarde is nie vir '{PropertyName}' nagekom nie.");
        registrar.Register(TemplatesNames.NotNull, afZACulture, "'{PropertyName}' mag nie leeg wees nie.");
        registrar.Register(TemplatesNames.MustBeNull, afZACulture, "'{PropertyName}' moet leeg wees.");
        registrar.Register(TemplatesNames.ChildValidator, afZACulture, "Nesvalideerder vir '{PropertyName}' het foute.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, afZACulture, "Voorwaarde of oombliklike opname stop die uitvoering van die oorblywende reëls in die huidige reëlset.");

        // French (fr-CA)
        CultureInfo frCACulture = CultureInfo.GetCultureInfo("fr-CA");
        registrar.Register(TemplatesNames.DefaultFailure, frCACulture, "La condition spécifiée n'a pas été respectée pour '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, frCACulture, "'{PropertyName}' ne doit pas être vide.");
        registrar.Register(TemplatesNames.MustBeNull, frCACulture, "'{PropertyName}' doit être vide.");
        registrar.Register(TemplatesNames.ChildValidator, frCACulture, "Le validateur imbriqué pour '{PropertyName}' a rencontré des échecs.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, frCACulture, "La condition ou le cliché a arrêté l'exécution des règles restantes dans le RuleSet actuel.");

        // German (de-CH)
        CultureInfo deCHCulture = CultureInfo.GetCultureInfo("de-CH");
        registrar.Register(TemplatesNames.DefaultFailure, deCHCulture, "Die angegebene Bedingung wurde für '{PropertyName}' nicht erfüllt.");
        registrar.Register(TemplatesNames.NotNull, deCHCulture, "'{PropertyName}' darf nicht leer sein.");
        registrar.Register(TemplatesNames.MustBeNull, deCHCulture, "'{PropertyName}' muss leer sein.");
        registrar.Register(TemplatesNames.ChildValidator, deCHCulture, "Verschachtelter Validator für '{PropertyName}' hat Fehler.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, deCHCulture, "Bedingung oder Schnappschuss stoppt die Ausführung der restlichen Regeln im aktuellen Regelwerk (RuleSet).");

        // Italian (it-CH)
        CultureInfo itCHCulture = CultureInfo.GetCultureInfo("it-CH");
        registrar.Register(TemplatesNames.DefaultFailure, itCHCulture, "La condizione specificata non è stata soddisfatta per '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, itCHCulture, "'{PropertyName}' non deve essere vuoto.");
        registrar.Register(TemplatesNames.MustBeNull, itCHCulture, "'{PropertyName}' deve essere vuoto.");
        registrar.Register(TemplatesNames.ChildValidator, itCHCulture, "Il validatore annidato per '{PropertyName}' ha riscontrato errori.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, itCHCulture, "La condizione o lo snapshot bloccano l'esecuzione delle regole rimanenti nel RuleSet corrente.");

        // Russian (ru-BY)
        CultureInfo ruBYCulture = CultureInfo.GetCultureInfo("ru-BY");
        registrar.Register(TemplatesNames.DefaultFailure, ruBYCulture, "Указанное условие не было выполнено для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ruBYCulture, "'{PropertyName}' не должен быть пустым.");
        registrar.Register(TemplatesNames.MustBeNull, ruBYCulture, "'{PropertyName}' должен быть пустым.");
        registrar.Register(TemplatesNames.ChildValidator, ruBYCulture, "Вложенный валидатор для '{PropertyName}' обнаружил ошибки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ruBYCulture, "Условие или моментальный снимок останавливают выполнение остальных правил в текущем RuleSet.");

        // Russian (ru-KZ)
        CultureInfo ruKZCulture = CultureInfo.GetCultureInfo("ru-KZ");
        registrar.Register(TemplatesNames.DefaultFailure, ruKZCulture, "Указанное условие не было выполнено для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ruKZCulture, "'{PropertyName}' не должен быть пустым.");
        registrar.Register(TemplatesNames.MustBeNull, ruKZCulture, "'{PropertyName}' должен быть пустым.");
        registrar.Register(TemplatesNames.ChildValidator, ruKZCulture, "Вложенный валидатор для '{PropertyName}' обнаружил ошибки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ruKZCulture, "Условие или моментальный снимок останавливают выполнение остальных правил в текущем RuleSet.");

        // Russian (ru-KG)
        CultureInfo ruKGCulture = CultureInfo.GetCultureInfo("ru-KG");
        registrar.Register(TemplatesNames.DefaultFailure, ruKGCulture, "Указанное условие не было выполнено для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ruKGCulture, "'{PropertyName}' не должен быть пустым.");
        registrar.Register(TemplatesNames.MustBeNull, ruKGCulture, "'{PropertyName}' должен быть пустым.");
        registrar.Register(TemplatesNames.ChildValidator, ruKGCulture, "Вложенный валидатор для '{PropertyName}' обнаружил ошибки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ruKGCulture, "Условие или моментальный снимок останавливают выполнение остальных правил в текущем RuleSet.");

        // Russian (ru-UA)
        CultureInfo ruUACulture = CultureInfo.GetCultureInfo("ru-UA");
        registrar.Register(TemplatesNames.DefaultFailure, ruUACulture, "Указанное условие не было выполнено для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ruUACulture, "'{PropertyName}' не должен быть пустым.");
        registrar.Register(TemplatesNames.MustBeNull, ruUACulture, "'{PropertyName}' должен быть пустым.");
        registrar.Register(TemplatesNames.ChildValidator, ruUACulture, "Вложенный валидатор для '{PropertyName}' обнаружил ошибки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ruUACulture, "Условие или моментальный снимок останавливают выполнение остальных правил в текущем RuleSet.");

        // Kazakh (kk-KZ)
        CultureInfo kkKZCulture = CultureInfo.GetCultureInfo("kk-KZ");
        registrar.Register(TemplatesNames.DefaultFailure, kkKZCulture, "Көрсетілген шарт '{PropertyName}' үшін орындалмады.");
        registrar.Register(TemplatesNames.NotNull, kkKZCulture, "'{PropertyName}' бос болмауы керек.");
        registrar.Register(TemplatesNames.MustBeNull, kkKZCulture, "'{PropertyName}' бос болуы керек.");
        registrar.Register(TemplatesNames.ChildValidator, kkKZCulture, "'{PropertyName}' үшін енгізілген валидатор қателерге тап болды.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, kkKZCulture, "Шарт немесе лездік сурет ағымдағы RuleSet-те қалған ережелердің орындалуын тоқтатады.");

        // Uzbek (uz-UZ)
        CultureInfo uzUZCulture = CultureInfo.GetCultureInfo("uz-UZ");
        registrar.Register(TemplatesNames.DefaultFailure, uzUZCulture, "Belgilangan shart '{PropertyName}' uchun bajarilmadi.");
        registrar.Register(TemplatesNames.NotNull, uzUZCulture, "'{PropertyName}' bo'sh bo'lmasligi kerak.");
        registrar.Register(TemplatesNames.MustBeNull, uzUZCulture, "'{PropertyName}' bo'sh bo'lishi kerak.");
        registrar.Register(TemplatesNames.ChildValidator, uzUZCulture, "'{PropertyName}' uchun joylashtirilgan tekshiruvchi xatolarga duch keldi.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, uzUZCulture, "Shart yoki surat joriy RuleSet-dagi qolgan qoidalarning bajarilishini to'xtatadi.");

        // Turkish (tr-CY)
        CultureInfo trCYCulture = CultureInfo.GetCultureInfo("tr-CY");
        registrar.Register(TemplatesNames.DefaultFailure, trCYCulture, "'{PropertyName}' için belirtilen koşul karşılanmadı.");
        registrar.Register(TemplatesNames.NotNull, trCYCulture, "'{PropertyName}' boş olmamalıdır.");
        registrar.Register(TemplatesNames.MustBeNull, trCYCulture, "'{PropertyName}' boş olmalıdır.");
        registrar.Register(TemplatesNames.ChildValidator, trCYCulture, "'{PropertyName}' için iç içe doğrulayıcıda hatalar oluştu.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, trCYCulture, "Koşul veya anlık görüntü, mevcut RuleSet'teki geri kalan kuralların yürütülmesini durdurur.");

        // Czech (cs-CZ)
        CultureInfo csCZCulture = CultureInfo.GetCultureInfo("cs-CZ");
        registrar.Register(TemplatesNames.DefaultFailure, csCZCulture, "Uvedená podmínka nebyla splněna pro '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, csCZCulture, "'{PropertyName}' nesmí být prázdné.");
        registrar.Register(TemplatesNames.MustBeNull, csCZCulture, "'{PropertyName}' musí být prázdné.");
        registrar.Register(TemplatesNames.ChildValidator, csCZCulture, "Vnořený validátor pro '{PropertyName}' narazil na chyby.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, csCZCulture, "Podmínka nebo snímek zastaví provádění zbývajících pravidel v aktuální sadě pravidel.");

        // Romanian (ro-RO)
        CultureInfo roROCulture = CultureInfo.GetCultureInfo("ro-RO");
        registrar.Register(TemplatesNames.DefaultFailure, roROCulture, "Condiția specificată nu a fost îndeplinită pentru '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, roROCulture, "'{PropertyName}' nu trebuie să fie gol.");
        registrar.Register(TemplatesNames.MustBeNull, roROCulture, "'{PropertyName}' trebuie să fie gol.");
        registrar.Register(TemplatesNames.ChildValidator, roROCulture, "Validatorul imbricat pentru '{PropertyName}' a întâmpinat erori.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, roROCulture, "Condiția sau instantaneul oprește executarea regulilor rămase în RuleSet-ul curent.");

        // Serbian (sr-Latn-RS)
        CultureInfo srRSLCulture = CultureInfo.GetCultureInfo("sr-Latn-RS");
        registrar.Register(TemplatesNames.DefaultFailure, srRSLCulture, "Navedeni uslov nije ispunjen za '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, srRSLCulture, "'{PropertyName}' ne sme biti prazno.");
        registrar.Register(TemplatesNames.MustBeNull, srRSLCulture, "'{PropertyName}' mora biti prazno.");
        registrar.Register(TemplatesNames.ChildValidator, srRSLCulture, "Ugnježdeni validator za '{PropertyName}' je naišao na greške.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, srRSLCulture, "Uslov ili snimak zaustavlja izvršavanje preostalih pravila u trenutnom RuleSet-u.");

        // Slovak (sk-SK)
        CultureInfo skSKCulture = CultureInfo.GetCultureInfo("sk-SK");
        registrar.Register(TemplatesNames.DefaultFailure, skSKCulture, "Uvedená podmienka nebola splnená pre '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, skSKCulture, "'{PropertyName}' nesmie byť prázdne.");
        registrar.Register(TemplatesNames.MustBeNull, skSKCulture, "'{PropertyName}' musí byť prázdne.");
        registrar.Register(TemplatesNames.ChildValidator, skSKCulture, "Vložený validátor pre '{PropertyName}' narazil na chyby.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, skSKCulture, "Podmienka alebo snímok zastaví vykonávanie zvyšných pravidiel v aktuálnom RuleSet.");

        // Bulgarian (bg-BG)
        CultureInfo bgBGCulture = CultureInfo.GetCultureInfo("bg-BG");
        registrar.Register(TemplatesNames.DefaultFailure, bgBGCulture, "Посоченото условие не беше изпълнено за '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, bgBGCulture, "'{PropertyName}' не трябва да бъде празен.");
        registrar.Register(TemplatesNames.MustBeNull, bgBGCulture, "'{PropertyName}' трябва да бъде празен.");
        registrar.Register(TemplatesNames.ChildValidator, bgBGCulture, "Вложен валидатор за '{PropertyName}' откри грешки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, bgBGCulture, "Условието или моментната снимка спират изпълнението на останалите правила в текущия RuleSet.");

        // Greek (el-GR)
        CultureInfo elGRCulture = CultureInfo.GetCultureInfo("el-GR");
        registrar.Register(TemplatesNames.DefaultFailure, elGRCulture, "Η καθορισμένη συνθήκη δεν ικανοποιήθηκε για το '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, elGRCulture, "Το '{PropertyName}' δεν πρέπει να είναι κενό.");
        registrar.Register(TemplatesNames.MustBeNull, elGRCulture, "Το '{PropertyName}' πρέπει να είναι κενό.");
        registrar.Register(TemplatesNames.ChildValidator, elGRCulture, "Ο ενσωματωμένος επικυρωτής για το '{PropertyName}' αντιμετώπισε αποτυχίες.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, elGRCulture, "Η συνθήκη ή το στιγμιότυπο σταματούν την εκτέλεση των υπολοίπων κανόνων στο τρέχον RuleSet.");

        // Swedish (sv-SE)
        CultureInfo svSECulture = CultureInfo.GetCultureInfo("sv-SE");
        registrar.Register(TemplatesNames.DefaultFailure, svSECulture, "Det angivna villkoret uppfylldes inte för '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, svSECulture, "'{PropertyName}' får inte vara tom.");
        registrar.Register(TemplatesNames.MustBeNull, svSECulture, "'{PropertyName}' måste vara tom.");
        registrar.Register(TemplatesNames.ChildValidator, svSECulture, "Kapslad validator för '{PropertyName}' stötte på fel.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, svSECulture, "Villkor eller ögonblicksbild stoppar exekveringen av de återstående reglerna i det aktuella RuleSet.");

        // Norwegian Bokmål (nb-NO)
        CultureInfo nbNOCulture = CultureInfo.GetCultureInfo("nb-NO");
        registrar.Register(TemplatesNames.DefaultFailure, nbNOCulture, "Den angitte betingelsen ble ikke oppfylt for '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, nbNOCulture, "'{PropertyName}' må ikke være tom.");
        registrar.Register(TemplatesNames.MustBeNull, nbNOCulture, "'{PropertyName}' må være tom.");
        registrar.Register(TemplatesNames.ChildValidator, nbNOCulture, "Nested validator for '{PropertyName}' møtte feil.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, nbNOCulture, "Betingelse eller øyeblikksbilde stopper utførelsen av de gjenværende reglene i gjeldende regelsett.");

        // Danish (da-DK)
        CultureInfo daDKCulture = CultureInfo.GetCultureInfo("da-DK");
        registrar.Register(TemplatesNames.DefaultFailure, daDKCulture, "Den angivne betingelse var ikke opfyldt for '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, daDKCulture, "'{PropertyName}' må ikke være tom.");
        registrar.Register(TemplatesNames.MustBeNull, daDKCulture, "'{PropertyName}' skal være tom.");
        registrar.Register(TemplatesNames.ChildValidator, daDKCulture, "Indlejret validator for '{PropertyName}' stødte på fejl.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, daDKCulture, "Betingelse eller øjebliksbillede stopper udførelsen af de resterende regler i det aktuelle RuleSet.");

        // Finnish (fi-FI)
        CultureInfo fiFICulture = CultureInfo.GetCultureInfo("fi-FI");
        registrar.Register(TemplatesNames.DefaultFailure, fiFICulture, "Määritetty ehto ei täyttynyt kohteelle '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, fiFICulture, "'{PropertyName}' ei saa olla tyhjä.");
        registrar.Register(TemplatesNames.MustBeNull, fiFICulture, "'{PropertyName}' on oltava tyhjä.");
        registrar.Register(TemplatesNames.ChildValidator, fiFICulture, "Sisäkkäinen validointi kohteelle '{PropertyName}' kohtasi virheitä.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, fiFICulture, "Ehto tai tilannekuva pysäyttää jäljellä olevien sääntöjen suorituksen nykyisessä RuleSet-joukossa.");

        // Hungarian (hu-HU)
        CultureInfo huHUCulture = CultureInfo.GetCultureInfo("hu-HU");
        registrar.Register(TemplatesNames.DefaultFailure, huHUCulture, "A megadott feltétel nem teljesült a(z) '{PropertyName}' számára.");
        registrar.Register(TemplatesNames.NotNull, huHUCulture, "A(z) '{PropertyName}' nem lehet üres.");
        registrar.Register(TemplatesNames.MustBeNull, huHUCulture, "A(z) '{PropertyName}'-nak üresnek kell lennie.");
        registrar.Register(TemplatesNames.ChildValidator, huHUCulture, "Beágyazott érvényesítő a(z) '{PropertyName}' számára hibákat talált.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, huHUCulture, "A feltétel vagy a pillanatkép leállítja a hátralévő szabályok végrehajtását az aktuális RuleSet-ben.");

        // Czech (cs-CZ)
        CultureInfo csCZCulture2 = CultureInfo.GetCultureInfo("cs-CZ");
        registrar.Register(TemplatesNames.DefaultFailure, csCZCulture2, "Uvedená podmínka nebyla splněna pro '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, csCZCulture2, "'{PropertyName}' nesmí být prázdné.");
        registrar.Register(TemplatesNames.MustBeNull, csCZCulture2, "'{PropertyName}' musí být prázdné.");
        registrar.Register(TemplatesNames.ChildValidator, csCZCulture2, "Vnořený validátor pro '{PropertyName}' narazil na chyby.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, csCZCulture2, "Podmínka nebo snímek zastaví provádění zbývajících pravidel v aktuální sadě pravidel.");

        // Slovak (sk-SK)
        CultureInfo skSKCulture2 = CultureInfo.GetCultureInfo("sk-SK");
        registrar.Register(TemplatesNames.DefaultFailure, skSKCulture2, "Uvedená podmienka nebola splnená pre '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, skSKCulture2, "'{PropertyName}' nesmie byť prázdne.");
        registrar.Register(TemplatesNames.MustBeNull, skSKCulture2, "'{PropertyName}' musí byť prázdne.");
        registrar.Register(TemplatesNames.ChildValidator, skSKCulture2, "Vložený validátor pre '{PropertyName}' narazil na chyby.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, skSKCulture2, "Podmienka alebo snímok zastaví vykonávanie zvyšných pravidiel v aktuálnom RuleSet.");

        // Belarusian (be-BY)
        CultureInfo beBYCulture = CultureInfo.GetCultureInfo("be-BY");
        registrar.Register(TemplatesNames.DefaultFailure, beBYCulture, "Указаная ўмова не была выканана для '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, beBYCulture, "'{PropertyName}' не павінна быць пустым.");
        registrar.Register(TemplatesNames.MustBeNull, beBYCulture, "'{PropertyName}' павінна быць пустым.");
        registrar.Register(TemplatesNames.ChildValidator, beBYCulture, "Укладзены валідатар для '{PropertyName}' выявіў памылкі.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, beBYCulture, "Умова або здымак спыняюць выкананне астатніх правілаў у бягучым RuleSet.");

        // Catalan (ca-ES)
        CultureInfo caESCulture = CultureInfo.GetCultureInfo("ca-ES");
        registrar.Register(TemplatesNames.DefaultFailure, caESCulture, "La condició especificada no es va complir per '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, caESCulture, "'{PropertyName}' no ha de ser buit.");
        registrar.Register(TemplatesNames.MustBeNull, caESCulture, "'{PropertyName}' ha de ser buit.");
        registrar.Register(TemplatesNames.ChildValidator, caESCulture, "El validador niuat per '{PropertyName}' ha trobat errors.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, caESCulture, "La condició o la instantània aturen l'execució de les regles restants al RuleSet actual.");

        // Albanian (sq-AL)
        CultureInfo sqALCulture = CultureInfo.GetCultureInfo("sq-AL");
        registrar.Register(TemplatesNames.DefaultFailure, sqALCulture, "Kushti i specifikuar nuk u plotësua për '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, sqALCulture, "'{PropertyName}' nuk duhet të jetë bosh.");
        registrar.Register(TemplatesNames.MustBeNull, sqALCulture, "'{PropertyName}' duhet të jetë bosh.");
        registrar.Register(TemplatesNames.ChildValidator, sqALCulture, "Vlejtësi i folezuar për '{PropertyName}' ka hasur dështime.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, sqALCulture, "Kushti ose momenti i ndezjes ndalojnë ekzekutimin e rregullave të mbetura në RuleSet-in aktual.");

        // Georgian (ka-GE)
        CultureInfo kaGECulture = CultureInfo.GetCultureInfo("ka-GE");
        registrar.Register(TemplatesNames.DefaultFailure, kaGECulture, "მითითებული პირობა არ დაკმაყოფილდა '{PropertyName}'-სთვის.");
        registrar.Register(TemplatesNames.NotNull, kaGECulture, "'{PropertyName}' არ უნდა იყოს ცარიელი.");
        registrar.Register(TemplatesNames.MustBeNull, kaGECulture, "'{PropertyName}' უნდა იყოს ცარიელი.");
        registrar.Register(TemplatesNames.ChildValidator, kaGECulture, "'{PropertyName}'-ის ჩაშენებულმა ვალიდატორმა აღმოაჩინა შეცდომები.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, kaGECulture, "პირობა ან სნაფშოტი აჩერებს დარჩენილი წესების შესრულებას მიმდინარე RuleSet-ში.");

        // Lithuanian (lt-LT)
        CultureInfo ltLTCulture = CultureInfo.GetCultureInfo("lt-LT");
        registrar.Register(TemplatesNames.DefaultFailure, ltLTCulture, "Nurodyta sąlyga nebuvo įvykdyta '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, ltLTCulture, "'{PropertyName}' negali būti tuščias.");
        registrar.Register(TemplatesNames.MustBeNull, ltLTCulture, "'{PropertyName}' turi būti tuščias.");
        registrar.Register(TemplatesNames.ChildValidator, ltLTCulture, "Įdėtasis validavimas '{PropertyName}' susidūrė su klaidomis.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, ltLTCulture, "Sąlyga arba momentinė nuotrauka sustabdo likusių taisyklių vykdymą dabartiniame taisyklių rinkinyje.");

        // Latvian (lv-LV)
        CultureInfo lvLVCulture = CultureInfo.GetCultureInfo("lv-LV");
        registrar.Register(TemplatesNames.DefaultFailure, lvLVCulture, "Norādītais nosacījums netika izpildīts '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, lvLVCulture, "'{PropertyName}' nedrīkst būt tukšs.");
        registrar.Register(TemplatesNames.MustBeNull, lvLVCulture, "'{PropertyName}' jābūt tukšam.");
        registrar.Register(TemplatesNames.ChildValidator, lvLVCulture, "Iegultais validatoram '{PropertyName}' radās kļūdas.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, lvLVCulture, "Nosacījums vai tūlītējs attēls aptur atlikušo noteikumu izpildi pašreizējā noteikumu kopā.");

        // Estonian (et-EE)
        CultureInfo etEECulture = CultureInfo.GetCultureInfo("et-EE");
        registrar.Register(TemplatesNames.DefaultFailure, etEECulture, "Määratud tingimus ei vastanud '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, etEECulture, "'{PropertyName}' ei tohi olla tühi.");
        registrar.Register(TemplatesNames.MustBeNull, etEECulture, "'{PropertyName}' peab olema tühi.");
        registrar.Register(TemplatesNames.ChildValidator, etEECulture, "Sügavuti valideerija '{PropertyName}' jaoks on tekkinud vigu.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, etEECulture, "Tingimus või hetktõmmis peatab järelejäänud reeglite täitmise praeguses reeglistikus.");

        // Armenian (hy-AM)
        CultureInfo hyAMCulture = CultureInfo.GetCultureInfo("hy-AM");
        registrar.Register(TemplatesNames.DefaultFailure, hyAMCulture, "Նշված պայմանը չբավարարվեց '{PropertyName}' համար։");
        registrar.Register(TemplatesNames.NotNull, hyAMCulture, "'{PropertyName}'-ը չպետք է լինի դատարկ։");
        registrar.Register(TemplatesNames.MustBeNull, hyAMCulture, "'{PropertyName}'-ը պետք է լինի դատարկ։");
        registrar.Register(TemplatesNames.ChildValidator, hyAMCulture, "'{PropertyName}' համար նախատեսված ներդրված վավերացուցիչը հանդիպեց անհաջողությունների։");
        registrar.Register(TemplatesNames.ConditionalFlowStop, hyAMCulture, "Պայմանը կամ նկարը դադարեցնում են ընթացիկ RuleSet-ում մնացած կանոնների իրականացումը։");

        // Azerbaijani (az-AZ)
        CultureInfo azAZCulture = CultureInfo.GetCultureInfo("az-AZ");
        registrar.Register(TemplatesNames.DefaultFailure, azAZCulture, "Müəyyən edilmiş şərt '{PropertyName}' üçün yerinə yetirilmədi.");
        registrar.Register(TemplatesNames.NotNull, azAZCulture, "'{PropertyName}' boş olmamalıdır.");
        registrar.Register(TemplatesNames.MustBeNull, azAZCulture, "'{PropertyName}' boş olmalıdır.");
        registrar.Register(TemplatesNames.ChildValidator, azAZCulture, "'{PropertyName}' üçün iç-içə validator uğursuzluqlara rast gəldi.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, azAZCulture, "Şərt və ya anlıq təsvir cari RuleSet-də qalan qaydaların icrasını dayandırır.");

        // Bosnian (bs-BA)
        CultureInfo bsBACulture = CultureInfo.GetCultureInfo("bs-BA");
        registrar.Register(TemplatesNames.DefaultFailure, bsBACulture, "Navedeni uvjet nije ispunjen za '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, bsBACulture, "'{PropertyName}' ne smije biti prazno.");
        registrar.Register(TemplatesNames.MustBeNull, bsBACulture, "'{PropertyName}' mora biti prazno.");
        registrar.Register(TemplatesNames.ChildValidator, bsBACulture, "Ugniježđeni validator za '{PropertyName}' je naišao na pogreške.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, bsBACulture, "Uvjet ili snimak zaustavlja izvršavanje preostalih pravila u trenutnom RuleSet-u.");

        // Croatian (hr-HR)
        CultureInfo hrHRCulture = CultureInfo.GetCultureInfo("hr-HR");
        registrar.Register(TemplatesNames.DefaultFailure, hrHRCulture, "Navedeni uvjet nije ispunjen za '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, hrHRCulture, "'{PropertyName}' ne smije biti prazno.");
        registrar.Register(TemplatesNames.MustBeNull, hrHRCulture, "'{PropertyName}' mora biti prazno.");
        registrar.Register(TemplatesNames.ChildValidator, hrHRCulture, "Ugniježđeni validator za '{PropertyName}' je naišao na greške.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, hrHRCulture, "Uvjet ili snimka zaustavlja izvršavanje preostalih pravila u trenutnom RuleSet-u.");

        // Slovenian (sl-SI)
        CultureInfo slSICulture = CultureInfo.GetCultureInfo("sl-SI");
        registrar.Register(TemplatesNames.DefaultFailure, slSICulture, "Navedeni pogoj ni bil izpolnjen za '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, slSICulture, "'{PropertyName}' ne sme biti prazno.");
        registrar.Register(TemplatesNames.MustBeNull, slSICulture, "'{PropertyName}' mora biti prazno.");
        registrar.Register(TemplatesNames.ChildValidator, slSICulture, "Ugnezdeni validator za '{PropertyName}' je naletel na napake.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, slSICulture, "Pogoj ali posnetek ustavi izvajanje preostalih pravil v trenutnem RuleSetu.");

        // Albanian (sq-AL)
        CultureInfo sqALCulture2 = CultureInfo.GetCultureInfo("sq-AL");
        registrar.Register(TemplatesNames.DefaultFailure, sqALCulture2, "Kushti i specifikuar nuk u plotësua për '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, sqALCulture2, "'{PropertyName}' nuk duhet të jetë bosh.");
        registrar.Register(TemplatesNames.MustBeNull, sqALCulture2, "'{PropertyName}' duhet të jetë bosh.");
        registrar.Register(TemplatesNames.ChildValidator, sqALCulture2, "Vlejtësi i folezuar për '{PropertyName}' ka hasur dështime.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, sqALCulture2, "Kushti ose momenti i ndezjes ndalojnë ekzekutimin e rregullave të mbetura në RuleSet-in aktual.");

        // Macedonian (mk-MK)
        CultureInfo mkMKCulture = CultureInfo.GetCultureInfo("mk-MK");
        registrar.Register(TemplatesNames.DefaultFailure, mkMKCulture, "Наведениот услов не е исполнет за '{PropertyName}'.");
        registrar.Register(TemplatesNames.NotNull, mkMKCulture, "'{PropertyName}' не смее да биде празен.");
        registrar.Register(TemplatesNames.MustBeNull, mkMKCulture, "'{PropertyName}' мора да биде празен.");
        registrar.Register(TemplatesNames.ChildValidator, mkMKCulture, "Вгнездениот валидатор за '{PropertyName}' наиде на грешки.");
        registrar.Register(TemplatesNames.ConditionalFlowStop, mkMKCulture, "Условот или сликата запираат со извршувањето на останатите правила во тековниот RuleSet.");
    }
}