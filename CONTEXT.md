# Diagnóstico de Instalação

Ferramenta Windows que avalia e, quando autorizado, corrige a saúde de uma instalação empacotada (serviços e runtimes da aplicação).

## Language

**Assistente de Reparo**:
Programa que detecta falhas na Instalação e oferece ou executa correções sob controle do operador.
_Avoid_: Health check puro, verificador passivo, coletor de evidências isolado

**Produto**:
Biblivre 5, sistema de gestão de bibliotecas cuja Instalação no Windows é criada pelo Instalador Oficial.
_Avoid_: UPA como nome do produto, stack genérica, “variante UPA” do instalador

**Instalador Oficial**:
Pacote de instalação Windows distribuído pelo projeto Biblivre, que cria a Instalação (Apache HTTPd, Tomcat, JRE, Postgres e o Contexto Biblivre5).
_Avoid_: Instalador UPA, setup customizado local como fonte da verdade

**Instalação**:
Conjunto coeso do Produto implantado no Windows pelo Instalador Oficial: Apache HTTPd, Tomcat, JRE, Postgres e o Contexto Biblivre5.
_Avoid_: Setup, deploy solto, máquina

**Operador**:
Pessoa não técnica (ex.: staff da biblioteca ou alguém seguindo um roteiro de suporte) que executa o Assistente de Reparo na máquina da Instalação. Não se espera que conheça nomes de serviços, paths ou JVM.
_Avoid_: Administrador de sistemas, DevOps, “usuário que sabe o que é Tomcat”

**Descoberta**:
Processo de localizar a Instalação varrendo **apenas drives fixos locais** em busca de um Padrão de Instalação.
_Avoid_: Registro Windows como fonte primária, path fixo único, escolha manual obrigatória, varredura de rede ou removíveis na v1

**Padrão de Instalação**:
Presença do diretório `Program Files\Apache Software Foundation\Tomcat 7.0\webapps\Biblivre5` na raiz de um drive (ex.: `C:\`, `D:\`).
_Avoid_: Heurística por “qualquer Tomcat”, busca só por WAR solto, path sem `Biblivre5`

**Contexto Biblivre5**:
Diretório (ou aplicação) implantado em `webapps\Biblivre5` que representa o Produto no Tomcat.
_Avoid_: WAR genérico, webapp, app

**Contexto Saudável**:
Contexto Biblivre5 que passa o Smoke HTTP tanto via Tomcat quanto via Apache — não basta a pasta existir no disco.
_Avoid_: “pasta presente”, só WEB-INF no disco sem prova HTTP

**Smoke HTTP (Tomcat)**:
Requisição local a `http://localhost:8080/Biblivre5/` que deve retornar HTTP 2xx e conter o Marcador de Login no corpo.
_Avoid_: Só código 200 de página default do Tomcat, root `/` sem `/Biblivre5`

**Smoke HTTP (Apache)**:
Requisição local a `http://localhost/Biblivre5/` que deve retornar HTTP 2xx e conter o Marcador de Login no corpo.
_Avoid_: Página default do Apache, só proxy “up” sem o contexto

**Marcador de Login**:
Trecho inequívoco da UI do Produto no corpo HTTP: `Core.submitForm('login', 'login', 'jsp')` (botão de login do Biblivre 5).
_Avoid_: Apenas a palavra “Biblivre”, title genérico, status HTTP sozinho

**Reparo**:
Ação corretiva que o Assistente de Reparo pode executar na Instalação, sempre sob controle do Operador.
_Avoid_: Reinstalação silenciosa, edição livre de configs, “consertar tudo”

**Catálogo de Reparos (v1)**:
Sequência interna (invisível ao Operador em jargão técnico): (1) iniciar serviços parados do Tomcat / Apache / Postgres; (2) reiniciar serviço quando o smoke indica falha; (3) gravar JVM Explícita quando a JVM Automática/path estiver inválida.
_Avoid_: Pedir ao Operador para escolher “Tomcat7” / “jvm.dll”, alteração de configs Apache/Postgres

**Modo Diagnóstico**:
Execução padrão: avalia a Instalação e explica o resultado em linguagem de efeito (“o Biblivre não abre”, “faltou o Java do servidor”), sem executar Reparos.
_Avoid_: Auto-reparo implícito, relatório cheio de jargão de stack

**Modo Reparo**:
Consentimento único em linguagem simples (efeito + risco, ex.: acesso pode ficar indisponível por alguns minutos); em seguida o Assistente aplica a sequência do Catálogo de Reparos sem nomear componentes ao Operador.
_Avoid_: Perguntas “reiniciar Tomcat?”, checkboxes técnicos, reparo totalmente silencioso sem nenhum “Sim”

**Consentimento de Reparo**:
Única autorização do Operador para entrar no Modo Reparo, formulada em efeito perceptível e risco (“Posso tentar corrigir? O acesso pode ficar fora por alguns minutos”), não em nomes de software. Aceito ⇒ o Assistente aplica a sequência interna do Catálogo de Reparos.
_Avoid_: Confirmação por componente, quiz técnico, reparo sem nenhum Sim

**Interface do Operador**:
Janela simples com ações em linguagem de efeito (“Verificar”, resultado curto, “Tentar corrigir?”), sem exigir conhecimento da stack.
_Avoid_: Console como UI principal, wizard longo, jargão na tela principal

**Relatório Técnico**:
Arquivo gerado para suporte remoto com evidências (Descoberta, serviços, Homes, JVM, smokes, Reparos tentados), separado da linguagem da Interface do Operador.
_Avoid_: Despejar log técnico na tela principal, relatório só verbal

**Elevação**:
Pedido de privilégio de administrador (UAC) apenas ao entrar no Modo Reparo. O Modo Diagnóstico e a geração do Relatório Técnico devem funcionar sem admin sempre que o Windows permitir a leitura necessária.
_Avoid_: Exigir admin só para abrir o Assistente, reparo sem elevação

**Seleção de Instalação**:
Quando a Descoberta encontra mais de um Padrão de Instalação, a Interface do Operador lista as opções só pela letra do disco (“Biblivre no disco C:”, “no disco D:”) e o Operador escolhe uma.
_Avoid_: Mostrar paths completos, escolher automaticamente o drive do sistema, reparar todas de uma vez

**Instalação Ausente**:
Resultado da Descoberta quando nenhum Padrão de Instalação é encontrado nos drives fixos locais. A Interface do Operador informa em linguagem simples que o Biblivre não foi encontrado; o Relatório Técnico registra o que foi varrido. Não há Modo Reparo sem Instalação.
_Avoid_: Pedir path técnico, oferecer reinstalação automática, falhar só com erro de programador

**Fora de Escopo (v1)**:
Reinstalar componentes, copiar/substituir WAR ou Contexto Biblivre5, alterar `httpd.conf` / dados do Postgres, ou qualquer correção além do Catálogo de Reparos (v1).
_Avoid_: Tratar alerta de diagnóstico como licença para mudar a Instalação à vontade

**Home do Tomcat**:
Diretório `...\Apache Software Foundation\Tomcat 7.0` inferido a partir do Contexto Biblivre5 encontrado na Descoberta.
_Avoid_: CATALINA_HOME genérico sem vínculo ao Produto

**Home do Apache**:
Diretório `Program Files (x86)\Apache Software Foundation\Apache2.2` no **mesmo drive** em que a Descoberta achou o Contexto Biblivre5.
_Avoid_: Qualquer httpd no PATH, Apache em drive diferente sem vínculo, Apache 2.4 genérico

**Home do Postgres**:
Diretório `Program Files\PostgreSQL\9.1` no **mesmo drive** em que a Descoberta achou o Contexto Biblivre5.
_Avoid_: Qualquer Postgres no sistema, versão diferente sem vínculo ao drive da Instalação

**JRE Candidata**:
Diretório cuja árvore contém `bin\server\jvm.dll` e `bin\java` (ou `bin\java.exe`), encontrado por varredura no sistema de arquivos.
_Avoid_: JAVA_HOME sozinho, java apenas no PATH, JDK sem jvm.dll em bin\server

**JRE Usável**:
JRE Candidata que passa o sanity check de execução (`java -version` / equivalente) com sucesso.
_Avoid_: “Java instalado” sem teste de execução, presença só de jvm.dll

**JVM do Tomcat**:
A `jvm.dll` que o serviço Windows do Tomcat está configurado para carregar (a mesma informação exposta no Tomcat7w, aba Java — ferramenta de diagnóstico técnico, não da UI do Operador).
_Avoid_: JAVA_HOME do usuário logado, java no PATH do prompt

**JVM Explícita**:
JVM do Tomcat definida por caminho absoluto para uma `jvm.dll` (equivalente a “Use default” desmarcado com path preenchido no Tomcat7w).
_Avoid_: Path vazio com Use default desmarcado

**JVM Automática**:
JVM do Tomcat deixada para resolução automática do serviço (“Use default” / `auto`), sem path explícito. É aceitável apenas enquanto a resolução resultar em JRE Usável; caso contrário o Assistente de Reparo deve gravar uma JVM Explícita apontando para uma JRE Usável.
_Avoid_: “sem Java configurado”, tratar path vazio com Use default desmarcado como Automática válida

**Serviço do Tomcat**:
Serviço Windows `Tomcat7` (display: “Apache Tomcat 7.0 Tomcat7”), criado pelo Instalador Oficial.
_Avoid_: Qualquer serviço com “Tomcat” no nome sem amarrar ao Home do Tomcat

**Serviço do Apache**:
Serviço Windows `Apache2.2`, criado pelo Instalador Oficial.
_Avoid_: Apache2.4 genérico, “Apache” sem número de versão do pacote

**Serviço do Postgres**:
Serviço Windows `postgresql-x64-9.1`, criado pelo Instalador Oficial.
_Avoid_: postgresql-x64 de outra versão, “PostgreSQL” genérico
