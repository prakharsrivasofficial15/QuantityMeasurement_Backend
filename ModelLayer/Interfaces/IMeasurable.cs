namespace ModelLayer.Interfaces
{
    public interface IMeasurable
    {
        string GetUnitName();
        double ConvertToBaseUnit(double value);
        double ConvertFromBaseUnit(double baseValue);
        bool SupportsArithmeticOperation() => true; // Default: supports arithmetic
    }
}