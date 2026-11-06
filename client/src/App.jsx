import { useState } from 'react';
import HotelPage from './pages/HotelPage';
import BookingPage from './pages/BookingPage';
import AvailabilityPage from './pages/AvailabilityPage';
import LookupPage from './pages/LookupPage';
import ReportPage from './pages/ReportPage';
const pages = [ ['hotel','Thông tin khách sạn',HotelPage], ['booking','Đặt phòng AI',BookingPage], ['availability','Kiểm tra phòng',AvailabilityPage], ['lookup','Tra cứu & hủy phòng',LookupPage], ['report','Báo cáo',ReportPage] ];
export default function App() {
  const [active, setActive] = useState('hotel');
  const Page = pages.find(([id]) => id === active)[2];
  return <><header className="top"><div className="top-inner"><a className="brand" href="/">☀ <span>SUNRISE HOTEL</span></a><span className="top-caption">TRỢ LÝ & QUẢN LÝ ĐẶT PHÒNG</span></div></header><main><div className="intro"><div><div className="eyebrow">CHÀO MỪNG ĐẾN SUNRISE HOTEL</div><h1>Chúng tôi có thể giúp gì cho bạn?</h1><p>Hỏi thông tin, đặt phòng và quản lý kỳ nghỉ của bạn tại Nha Trang.</p></div><div className="intro-icon" aria-hidden="true">✳</div></div><nav className="tabs" aria-label="Các chức năng">{pages.map(([id,label]) => <button type="button" key={id} className={`tab${active===id ? ' active' : ''}`} aria-current={active===id ? 'page' : undefined} onClick={() => setActive(id)}>{label}</button>)}</nav><Page /><div className="bottom-note"><span>✳</span> SUNRISE HOTEL · 25 TRẦN PHÚ, NHA TRANG</div></main></>;
}
