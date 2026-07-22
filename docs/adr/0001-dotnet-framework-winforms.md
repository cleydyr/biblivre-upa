# .NET Framework + WinForms para o Assistente de Reparo

Precisamos de uma Interface do Operador (GUI simples) que rode em Windows 7 ou posterior. Escolhemos **.NET Framework 4.8 (ou 4.7.2) + WinForms** em vez de .NET moderno self-contained (sem suporte oficial ao Windows 7) ou PowerShell como UI principal (pior para Operador não técnico). O runtime .NET 4.x pode precisar estar presente ou ser distribuído à parte no kit de suporte.

## Considered Options

- .NET Framework + WinForms — aceito
- PowerShell + UI mínima — rejeitado (UX frágil para Operador não técnico)
- .NET 6/8 self-contained — rejeitado (quebra o requisito Windows 7)
- Go/Rust + GUI nativa — rejeitado (custo alto para o escopo da UI)
