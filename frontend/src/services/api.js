const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim()
const apiBaseUrl = configuredBaseUrl ? configuredBaseUrl.replace(/\/$/, '') : ''

export async function getPlayerAssets({ signal } = {}) {
  const response = await fetch(`${apiBaseUrl}/api/getassetsbyplayer`, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    let message = `The server returned HTTP ${response.status}.`

    try {
      const problem = await response.json()
      if (problem?.message) message = problem.message
    } catch {
      // Preserve the HTTP status message when the response has no JSON body.
    }

    throw new Error(message)
  }

  return response.json()
}
