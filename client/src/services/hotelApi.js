export async function request(path, options = {}) {
  const response = await fetch(path, options);
  let data;
  try { data = await response.json(); } catch { throw new Error('Máy chủ trả về dữ liệu không hợp lệ.'); }
  if (!response.ok) throw new Error(data.error || 'Không thể xử lý yêu cầu. Vui lòng thử lại.');
  return data;
}
const post = (path, body) => request(path, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
export const hotelApi = {
  ask: question => post('/api/ai/ask', { question }),
  book: text => post('/api/bookings/parse', { text }),
  chat: message => post('/api/assistant/chat', { message }),
  report: (from, to) => request(`/api/reports/bookings?from=${encodeURIComponent(from)}&to=${encodeURIComponent(to)}`),
};
export const money = value => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
export const dateText = value => value ? value.split('-').reverse().join('/') : '';
