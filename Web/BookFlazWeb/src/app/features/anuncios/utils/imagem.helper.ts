/**
 * Utis relacionados com upload e validação de imagens.
 *
 * Este módulo centraliza:
 * - Limites de quantidade de imagens por anúncio;
 * - Limites de tamanho de ficheiro (em MB e bytes);
 * - Formatos suportados;
 * - Funções de validação e geração de preview para imagens.
 */

export const MAX_IMAGENS = 5; // Número máximo de imagens permitidas por anúncio.
export const MAX_TAMANHO_MB = 5; // Tamanho máximo permitido para cada imagem, em megabytes (MB).
export const MAX_TAMANHO_BYTES = MAX_TAMANHO_MB * 1024 * 1024; // Tamanho máximo permitido para cada imagem, em bytes.

export const FORMATOS_ACEITES = ['image/jpeg', 'image/png', 'image/jpg']; // Lista de formatos de imagem permitidos

/**
 * Valida um ficheiro de imagem de acordo com as regras de negócio definidas.
 *
 * Regras aplicadas:
 *  1. Verifica se o tipo do ficheiro está incluído em {@link FORMATOS_ACEITES}.
 *  2. Verifica se o tamanho do ficheiro não ultrapassa {@link MAX_TAMANHO_BYTES}.
 *
 * Se a imagem for válida, devolve `null`.
 * Caso contrário, devolve uma mensagem de erro descritiva adequada para ser
 * apresentada diretamente ao utilizador.
 *
 * @param file - Ficheiro de imagem selecionado pelo utilizador.
 * @returns `null` se a imagem for válida; caso contrário, uma string com a descrição do erro.
 */
export function validarImagem(file: File): string | null {

    // Verificação do tipo ficheiro
    if (!FORMATOS_ACEITES.includes(file.type)) {
        return `Formato inválido: ${file.name}`;
    }

    // Verificação do tamanho do ficheiro
    if (file.size > MAX_TAMANHO_BYTES) {
        return `${file.name} excede o limite de ${MAX_TAMANHO_MB}MB.`;
    }

    return null; // retorna null em caso de sucesso
}

/**
 * Gera uma representação em base64 de um ficheiro de imagem.
 *
 * Esta função é útil para:
 *  - Mostrar previews das imagens no frontend antes do envio para o servidor;
 *  - Atualizar uma lista de thumbnails em componentes de upload;
 *  - Validar visualmente o conteúdo selecionado pelo utilizador.
 *
 * Implementação:
 *  - Utiliza a API `FileReader` do browser;
 *  - Lê o conteúdo do ficheiro como Data URL (`readAsDataURL`);
 *  - Resolve a Promise com o resultado (`string`) quando a leitura termina.
 *
 * @param file - Ficheiro de imagem a ser convertido para Data URL.
 * @returns Promise que resolve com uma string contendo o Data URL da imagem.
 */
export function gerarPreview(file: File): Promise<string> {
    return new Promise(resolve => {
        const reader = new FileReader();

        // Quando terminar a leitura, devolvemos o resultado (base64)
        reader.onload = (e: any) => resolve(e.target?.result as string);

        // Leitura como DataURL (necessário para usar em <img src="...">)
        reader.readAsDataURL(file);
    })
}