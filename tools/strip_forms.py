from pathlib import Path
p = Path(r'E:\DoAn\RPMS\tools\RpmsTestExec\Program.cs')
text = p.read_text(encoding='utf-8')
keep = ('MainForm', 'LoginForm', 'RegisterForm')
lines = text.splitlines(True)
out = []
for line in lines:
    if 'GetRequiredService<' in line and 'Form' in line and not any(k in line for k in keep):
        if 'using var' in line or 'GetRequiredService<' in line:
            continue
    out.append(line)
p.write_text(''.join(out), encoding='utf-8')
print('remaining Form resolves:')
for ln in p.read_text(encoding='utf-8').splitlines():
    if 'GetRequiredService<' in ln and 'Form' in ln:
        print(' ', ln.strip())
