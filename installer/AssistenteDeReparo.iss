; Inno Setup 6 — Assistente de Reparo Biblivre 5
; Compilar no Windows (CI ou máquina local). Não funciona neste Mac.

#define MyAppName "Assistente de Reparo Biblivre"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Biblivre"
#define MyAppExeName "AssistenteDeReparo.exe"
#define MyAppSource "..\src\AssistenteDeReparo\bin\Release\net48"

[Setup]
AppId={{E8F3A2B1-9C4D-4F6A-8E1B-7D5C3A2F9010}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\AssistenteDeReparoBiblivre
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=AssistenteDeReparo-Biblivre-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; Operador não técnico: instalar sem admin; UAC só no Modo Reparo do app
PrivilegesRequired=lowest
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=6.1sp1
SetupIconFile=
UninstallDisplayIcon={app}\{#MyAppExeName}
InfoBeforeFile=
LicenseFile=
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar ícone na área de trabalho"; GroupDescription: "Atalhos:"; Flags: unchecked

[Files]
Source: "{#MyAppSource}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSource}\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion
; Dependências satélite do build (se existirem)
Source: "{#MyAppSource}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir o Assistente agora"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
end;
