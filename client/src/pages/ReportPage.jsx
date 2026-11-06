import { useEffect, useState } from 'react';
import { Heading } from '../components/Common';
import { hotelApi, money, dateText } from '../services/hotelApi';
const iso=d=>`${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;
export default function ReportPage() {
 const now=new Date(); const [from,setFrom]=useState(iso(new Date(now.getFullYear(),now.getMonth(),1))),[to,setTo]=useState(iso(new Date(now.getFullYear(),now.getMonth()+1,0))),[rows,setRows]=useState([]),[status,setStatus]=useState('Đang tải…');
 async function load(a,b){setStatus('Đang tải…');try{if(a>b)throw new Error('Từ ngày phải trước hoặc bằng đến ngày.');const data=await hotelApi.report(a,b);setRows(data);setStatus(data.length?`${data.length} lượt đặt phòng`:'Không có đặt phòng nào trong khoảng ngày này.');}catch(e){setRows([]);setStatus(e.message||'Không thể kết nối máy chủ.');}}
 useEffect(()=>{load(from,to)},[]);
 return <section className="panel"><Heading eyebrow="TỔNG QUAN HOẠT ĐỘNG" title="Báo cáo đặt phòng" description="Theo dõi các lượt đặt phòng đã xác nhận theo ngày nhận phòng." badge="QUẢN LÝ"/><form className="filters" onSubmit={e=>{e.preventDefault();load(from,to)}}><label>Từ ngày<input type="date" required value={from} onChange={e=>setFrom(e.target.value)}/></label><label>Đến ngày<input type="date" required value={to} onChange={e=>setTo(e.target.value)}/></label><button type="submit">Xem báo cáo <span>→</span></button></form><p id="status" role="status">{status}</p><div className="tablewrap"><table><thead><tr>{['STT','Phòng','Loại phòng','Khách hàng','Nhận phòng','Trả phòng','Số đêm','Tổng tiền'].map(x=><th key={x}>{x}</th>)}</tr></thead><tbody>{rows.map(row=><tr key={row.no}><td>{row.no}</td><td>{row.roomNumber}</td><td>{row.roomType}</td><td>{row.guestName}</td><td>{dateText(row.checkIn)}</td><td>{dateText(row.checkOut)}</td><td>{row.nights}</td><td>{money(row.totalPrice)}</td></tr>)}</tbody></table></div></section>;
}
