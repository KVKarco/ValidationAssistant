namespace KVKarco.ValidationAssistant;

//used for di keyed dependencies so validators for some type of T can be distinguished
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ValidatorNameAttribute : Attribute
{
    private readonly string _validatorName;

    public ValidatorNameAttribute(string validatorName)
    {
        _validatorName = validatorName;
    }

    public string ValidatorName => _validatorName;
}
