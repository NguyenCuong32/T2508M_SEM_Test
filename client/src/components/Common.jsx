import { useState } from 'react';
export function useRequest() {
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);
  async function run(action) {
    setLoading(true); setResult(null);
    try { setResult({ content: await action(), error: false }); }
    catch (error) { setResult({ content: error.message || 'Không thể kết nối máy chủ.', error: true }); }
    finally { setLoading(false); }
  }
  return { result, loading, run };
}
export function Result({ result, loading }) {
  if (loading) return <div className="result" role="status">Đang xử lý yêu cầu…</div>;
  if (!result) return null;
  return <div className={`result${result.error ? ' error' : ''}`} role="status">{result.content}</div>;
}
export function Heading({ eyebrow, title, description, badge }) {
  return <div className="section-heading"><div><div className="eyebrow">{eyebrow}</div><h2>{title}</h2><p>{description}</p></div>{badge && <span className="badge">{badge}</span>}</div>;
}
export function Submit({ loading, children, className }) { return <button type="submit" className={className} disabled={loading}>{loading ? 'Đang xử lý…' : children} <span>→</span></button>; }
