(function () {
  "use strict";

  var REPO = "cleydyr/biblivre-upa";
  var RELEASES_URL = "https://github.com/" + REPO + "/releases/latest";

  /* ---------- Última versão: aponta o botão direto para o instalador ---------- */

  function formatarTamanho(bytes) {
    if (!bytes) return "";
    var mb = bytes / (1024 * 1024);
    return mb.toLocaleString("pt-BR", { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + " MB";
  }

  function atualizarBotoes(release) {
    var asset = null;
    (release.assets || []).forEach(function (a) {
      if (!asset && /\.exe$/i.test(a.name)) asset = a;
    });
    var versao = (release.tag_name || "").replace(/^v/i, "");
    var nota = document.getElementById("baixar-nota");

    var links = document.querySelectorAll("#baixar, [data-baixar]");
    Array.prototype.forEach.call(links, function (a) {
      a.href = asset ? asset.browser_download_url : RELEASES_URL;
    });

    if (nota) {
      var partes = [];
      if (versao) partes.push("<strong>Versão " + versao + "</strong>");
      if (asset && asset.size) partes.push(formatarTamanho(asset.size));
      partes.push("Windows 7 ou mais recente");
      nota.innerHTML = partes.join(" · ");
    }
  }

  if (window.fetch) {
    fetch("https://api.github.com/repos/" + REPO + "/releases/latest", {
      headers: { Accept: "application/vnd.github+json" }
    })
      .then(function (r) { return r.ok ? r.json() : null; })
      .then(function (release) { if (release) atualizarBotoes(release); })
      .catch(function () { /* mantém o link para a página de releases */ });
  }

  /* ---------- Simulação da janela do programa ---------- */

  var janela = document.getElementById("janela");
  if (!janela) return;

  var el = {
    status: document.getElementById("j-status"),
    resultado: document.getElementById("j-resultado"),
    progresso: document.getElementById("j-progresso"),
    verificar: document.getElementById("j-verificar"),
    corrigir: document.getElementById("j-corrigir"),
    relatorio: document.getElementById("j-relatorio"),
    dialogo: document.getElementById("j-dialogo"),
    sim: document.getElementById("j-sim"),
    replay: document.getElementById("replay")
  };

  var reduzMovimento = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  var timers = [];
  var rodando = false;

  var CAMINHO_RELATORIO = "C:\\Users\\biblioteca\\Desktop\\Biblivre-Relatorios\\relatorio-biblivre-20260907-101532.txt";

  var cenas = {
    inicio: function () {
      el.status.textContent = "Clique em Verificar para analisar este computador.";
      el.status.className = "janela-status";
      el.resultado.textContent = "";
      el.progresso.hidden = true;
      el.corrigir.disabled = true;
      el.relatorio.disabled = true;
      el.dialogo.hidden = true;
      limparAtivos();
    },
    verificando: function () {
      el.verificar.classList.add("ativo");
      el.status.textContent = "Procurando o Biblivre e verificando se está funcionando…";
      el.status.className = "janela-status";
      el.resultado.textContent = "";
      el.progresso.hidden = false;
    },
    problemas: function () {
      limparAtivos();
      el.progresso.hidden = true;
      el.status.textContent = "Encontrei problemas.";
      el.status.className = "janela-status j-atencao";
      el.resultado.textContent =
        "• O Biblivre não abre pelo endereço usual no navegador.\n" +
        "• O acesso pela rede local (servidor web) não está em execução.";
      el.corrigir.disabled = false;
      el.relatorio.disabled = false;
    },
    pedirConsentimento: function () {
      el.corrigir.classList.add("ativo");
      el.dialogo.hidden = false;
    },
    consentir: function () {
      el.sim.classList.add("ativo");
    },
    corrigindo: function () {
      el.dialogo.hidden = true;
      limparAtivos();
      el.status.textContent = "Tentando corrigir… isso pode levar alguns minutos.";
      el.status.className = "janela-status";
      el.resultado.textContent = "";
      el.progresso.hidden = false;
    },
    pronto: function () {
      el.progresso.hidden = true;
      el.status.textContent = "Pronto — o Biblivre parece estar funcionando.";
      el.status.className = "janela-status j-ok";
      el.resultado.textContent =
        "Correção concluída.\n\n" +
        "Ações realizadas (detalhe técnico no relatório).\n\n" +
        "Relatório salvo em:\n" + CAMINHO_RELATORIO;
      el.corrigir.disabled = true;
      el.relatorio.disabled = false;
    }
  };

  function limparAtivos() {
    [el.verificar, el.corrigir, el.sim].forEach(function (b) { b.classList.remove("ativo"); });
  }

  function limparTimers() {
    timers.forEach(clearTimeout);
    timers = [];
  }

  function agendar(fn, ms) {
    timers.push(setTimeout(fn, ms));
  }

  var roteiro = [
    ["inicio", 0],
    ["verificando", 900],
    ["problemas", 3200],
    ["pedirConsentimento", 4800],
    ["consentir", 6300],
    ["corrigindo", 6650],
    ["pronto", 9300]
  ];

  function tocar() {
    limparTimers();
    if (reduzMovimento) {
      cenas.inicio();
      cenas.problemas();
      cenas.pronto();
      return;
    }
    rodando = true;
    roteiro.forEach(function (passo) {
      agendar(function () {
        cenas[passo[0]]();
        if (passo[0] === "pronto") rodando = false;
      }, passo[1]);
    });
  }

  if (el.replay) el.replay.addEventListener("click", tocar);

  janela.addEventListener("click", function () {
    if (!rodando) tocar();
  });

  /* começa quando a janela entra na tela, para o visitante ver desde o início */
  if ("IntersectionObserver" in window && !reduzMovimento) {
    cenas.inicio();
    var obs = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (e.isIntersecting) {
          obs.disconnect();
          agendar(tocar, 500);
        }
      });
    }, { threshold: 0.5 });
    obs.observe(janela);
  } else {
    tocar();
  }
})();
