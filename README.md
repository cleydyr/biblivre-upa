# Assistente de Reparo — Biblivre 5

Programa Windows (7+) que verifica e, com consentimento, tenta corrigir a instalação criada pelo instalador oficial do Biblivre 5.

Requisitos de domínio: ver [`CONTEXT.md`](CONTEXT.md).  
Decisão de stack: [`docs/adr/0001-dotnet-framework-winforms.md`](docs/adr/0001-dotnet-framework-winforms.md).

## Requisitos para build

- Windows 7 SP1 ou posterior
- [.NET Framework 4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48) (ou Visual Studio com workload .NET desktop)
- SDK .NET com suporte a `net48`, **ou** Visual Studio 2019/2022

## Compilar

Na pasta do repositório, no Windows:

```bat
dotnet build AssistenteDeReparo.sln -c Release
```

O executável sai em:

`src\AssistenteDeReparo\bin\Release\net48\AssistenteDeReparo.exe`

## Instalador Windows

O app é .NET Framework 4.8 + WinForms e o empacotador (Inno Setup) só roda em Windows.

### GitHub Actions → Release

O workflow [`.github/workflows/build-windows-installer.yml`](.github/workflows/build-windows-installer.yml) roda em `windows-latest`, gera o Setup e **anexa o `.exe` a uma GitHub Release**.

**Disparos:**
- Push de tag `v*` (ex.: `git tag v1.0.0 && git push origin v1.0.0`)
- Manual: **Actions** → **Build Windows Installer** → **Run workflow** (opcional informar a versão)

**Onde baixar:** página **Releases** do repositório → asset `AssistenteDeReparo-Biblivre-Setup-*.exe`  
(também fica em Artifacts do run, por 30 dias)

### Máquina Windows (opcional)

Com .NET 4.8 + [Inno Setup 6](https://jrsoftware.org/isinfo.php):

```powershell
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
```

Saída: `dist\AssistenteDeReparo-Biblivre-Setup-1.0.0.exe`  
Instala em `%LocalAppData%\AssistenteDeReparoBiblivre` **sem** exigir admin (o UAC aparece só ao **Tentar corrigir**).

## Uso (Operador)

1. Abrir `AssistenteDeReparo.exe`
2. **Verificar**
3. Se houver mais de um disco com Biblivre, escolher o disco e verificar de novo
4. Se houver problemas, **Tentar corrigir** (UAC só nesta etapa)
5. **Salvar relatório** para enviar ao suporte (também em `Desktop\Biblivre-Relatorios\`)

## Reparos (v1)

- Iniciar serviços parados (`Tomcat7`, `Apache2.2`, `postgresql-x64-9.1`)
- Reiniciar serviço se o smoke HTTP ainda falhar
- Gravar JVM explícita no Procrun quando a configuração estiver inválida/morta
