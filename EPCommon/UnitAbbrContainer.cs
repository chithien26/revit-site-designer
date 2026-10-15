namespace EPCommon;

public class UnitAbbrContainer
{
	private string m_strAbbreviation;

	private double m_dEnglishFactor;

	private double m_dMetricFactor;

	public string Abbreviation => m_strAbbreviation;

	public double EnglishFactor => m_dEnglishFactor;

	public double MetricFactor => m_dMetricFactor;

	public UnitAbbrContainer(string _strAbbreviation, double _dEnglishFactor, double _dMetricFactor)
	{
		m_strAbbreviation = _strAbbreviation;
		m_dEnglishFactor = _dEnglishFactor;
		m_dMetricFactor = _dMetricFactor;
	}
}
