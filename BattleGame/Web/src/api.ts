export interface PlayerAssetRow {
  no: number
  playerName: string
  level: number
  age: string
  assetName: string
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:7071/api'

export async function getAssetsByPlayer(signal?: AbortSignal): Promise<PlayerAssetRow[]> {
  const res = await fetch(`${API_BASE_URL}/getassetsbyplayer`, { signal })
  if (!res.ok) throw new Error(`Request failed with status ${res.status}`)
  return res.json()
}
