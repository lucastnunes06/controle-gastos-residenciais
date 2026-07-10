type ApiError = {
  detail?: string;
  message?: string;
  title?: string;
  errors?: Record<string, string[]>;
};

/**
 * Centraliza chamadas HTTP e converte respostas Problem Details em mensagens
 * que a interface consegue apresentar sem expor detalhes técnicos.
 */
export async function api<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
  });

  if (!response.ok) {
    const body = (await response.json().catch(() => ({}))) as ApiError;
    const validationMessage = body.errors
      ? Object.values(body.errors).flat()[0]
      : undefined;

    throw new Error(
      validationMessage ??
        body.detail ??
        body.message ??
        body.title ??
        "Não foi possível concluir a operação. Tente novamente.",
    );
  }

  return response.status === 204
    ? (undefined as T)
    : (response.json() as Promise<T>);
}
