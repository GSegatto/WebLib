// Preferências do usuário (tema e tamanho do texto) guardadas no navegador.
(function () {
    const ler = (k, padrao) => { try { return localStorage.getItem(k) || padrao; } catch (e) { return padrao; } };
    const gravar = (k, v) => { try { localStorage.setItem(k, v); } catch (e) { } };
    const escuroSistema = () => window.matchMedia('(prefers-color-scheme: dark)').matches;
    const efetivo = p => (p === 'escuro' || (p === 'auto' && escuroSistema())) ? 'dark' : 'light';

    function rotulos() {
        const escuro = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        document.querySelectorAll('[data-tema-toggle]').forEach(b => {
            b.textContent = escuro ? '☀️ Modo claro' : '🌙 Modo escuro';
        });
    }
    function aplicarTema(p) {
        document.documentElement.setAttribute('data-bs-theme', efetivo(p));
        rotulos();
    }
    function aplicarFonte(f) {
        document.documentElement.style.fontSize = f === 'grande' ? '112.5%' : '';
    }

    // Botão rápido do menu de perfil
    document.querySelectorAll('[data-tema-toggle]').forEach(b => b.addEventListener('click', () => {
        const escuro = document.documentElement.getAttribute('data-bs-theme') === 'dark';
        const novo = escuro ? 'claro' : 'escuro';
        gravar('tema', novo);
        aplicarTema(novo);
        document.querySelectorAll('input[name="tema"]').forEach(r => r.checked = (r.value === novo));
    }));

    // Página Preferências
    document.querySelectorAll('input[name="tema"]').forEach(r => {
        r.checked = (r.value === ler('tema', 'auto'));
        r.addEventListener('change', () => { gravar('tema', r.value); aplicarTema(r.value); });
    });
    document.querySelectorAll('input[name="fonte"]').forEach(r => {
        r.checked = (r.value === ler('fonte', 'normal'));
        r.addEventListener('change', () => { gravar('fonte', r.value); aplicarFonte(r.value); });
    });

    // Modo "automático" acompanha o sistema operacional
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
        if (ler('tema', 'auto') === 'auto') aplicarTema('auto');
    });

    rotulos();
})();
