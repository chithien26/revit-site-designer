from pathlib import Path
import re

root = Path(__file__).resolve().parent.parent
dest = root / 'Compatibility'
dest.mkdir(exist_ok=True)
enums = ['UnitType', 'DisplayUnitType', 'UnitSymbolType', 'ParameterType', 'BuiltInParameterGroup']
text = 'namespace EPCommon;\n\n// Stable values used by the original settings and geometry code.\n'
for name in enums:
    source = (root / 'tools' / (name + '.cs')).read_text(encoding='utf-8-sig')
    source = source[source.index('public enum'):]
    source = re.sub(r'^\s*///.*\n', '', source, flags=re.M)
    text += source + '\n'
(dest / 'LegacyUnitTypes.cs').write_text(text, encoding='utf-8')

for path in (root / 'EPCommon').glob('*.cs'):
    s = path.read_text(encoding='utf-8-sig')
    for prop, method in [('DisplayUnits','LegacyDisplayUnits'), ('UnitSymbol','LegacyUnitSymbol')]:
        s = re.sub(r'(\w+)\.'+prop+r'\s*=\s*([^;]+);', r'\1.Set'+method+r'(\2);', s)
        s = re.sub(r'\.'+prop+r'\b', '.'+method+'()', s)
    s = s.replace('UnitFormatUtils.TryParse(', 'LegacyUnits.TryParse(').replace('UnitFormatUtils.Format(', 'LegacyUnits.Format(')
    s = s.replace('parameter.Definition.ParameterType', 'parameter.Definition.LegacyParameterType()')
    path.write_text(s, encoding='utf-8')
