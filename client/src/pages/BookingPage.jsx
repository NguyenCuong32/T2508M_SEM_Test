import { useState } from 'react';
import { Heading, Result, Submit, useRequest } from '../components/Common';
import { hotelApi, money, dateText } from '../services/hotelApi';
export default function BookingPage() {
 const [text,setText]=useState(''); const {result,loading,run}=useRequest();
 async function book() { const {parsed,booking}=await hotelApi.book(text.trim()); return `Đặt phòng thành công!\nMã booking: ${booking.bookingId}\nKhách hàng: ${booking.guestName}\nSố điện thoại: ${booking.phone}\nLoại phòng: ${parsed.roomType} · ${parsed.guests} khách\nNhận phòng: ${dateText(booking.checkIn)} · Trả phòng: ${dateText(booking.checkOut)}\nTổng tiền: ${money(booking.totalPrice)}\nTrạng thái: Đã xác nhận`; }
 return <section className="panel"><Heading eyebrow="ĐẶT PHÒNG DỄ DÀNG" title="Đặt phòng bằng AI" description="Viết yêu cầu bằng tiếng Việt. Hệ thống sẽ tìm phòng phù hợp và xác nhận đặt phòng." badge="HỖ TRỢ BỞI AI"/><form onSubmit={e=>{e.preventDefault();if(text.trim())run(book)}}><label htmlFor="booking-text">Yêu cầu đặt phòng</label><textarea id="booking-text" required rows="5" value={text} onChange={e=>setText(e.target.value)} placeholder="Tôi là Nguyễn Văn An, số điện thoại 0901234567, muốn đặt phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người."/><p className="field-help">Vui lòng ghi rõ họ tên, số điện thoại, loại phòng, ngày nhận và trả phòng, số khách.</p><div className="actions"><Submit loading={loading}>Tìm và đặt phòng</Submit></div></form><Result result={result} loading={loading}/></section>;
}
