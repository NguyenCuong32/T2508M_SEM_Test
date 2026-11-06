import { useState } from 'react';
import { Heading, Result, Submit, useRequest } from '../components/Common';
import { hotelApi } from '../services/hotelApi';
export default function AvailabilityPage() {
 const [type,setType]=useState('Single'),[start,setStart]=useState(''),[end,setEnd]=useState(''); const {result,loading,run}=useRequest();
 async function check(){if(end<=start)throw new Error('Ngày trả phòng phải sau ngày nhận phòng.');return (await hotelApi.chat(`Kiểm tra phòng loại ${type} còn trống từ ${start} đến ${end}. Cho biết số phòng và giá mỗi đêm.`)).answer;}
 return <section className="panel"><Heading eyebrow="LÊN KẾ HOẠCH CHUYẾN ĐI" title="Kiểm tra phòng trống" description="Chọn loại phòng và thời gian lưu trú để trợ lý kiểm tra phòng và giá."/><form onSubmit={e=>{e.preventDefault();run(check)}}><div className="fields"><label>Loại phòng<select value={type} onChange={e=>setType(e.target.value)}><option value="Single">Single · Phòng đơn</option><option value="Double">Double · Phòng đôi</option><option value="Suite">Suite</option></select></label><label>Ngày nhận phòng<input type="date" required value={start} onChange={e=>setStart(e.target.value)}/></label><label>Ngày trả phòng<input type="date" required value={end} onChange={e=>setEnd(e.target.value)}/></label></div><div className="actions"><Submit loading={loading}>Kiểm tra phòng</Submit></div></form><Result result={result} loading={loading}/></section>;
}
