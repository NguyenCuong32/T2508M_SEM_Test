import { useState } from 'react';
import { Heading, Result, Submit, useRequest } from '../components/Common';
import { hotelApi } from '../services/hotelApi';
export default function HotelPage() {
 const [question,setQuestion] = useState(''); const {result,loading,run}=useRequest();
 const suggestions=['Giờ nhận & trả phòng','Tiện ích khách sạn','Địa chỉ khách sạn'];
 const questions=['Khách sạn nhận phòng và trả phòng lúc mấy giờ?','Khách sạn có những tiện ích gì?','Khách sạn ở địa chỉ nào?'];
 return <section className="panel"><Heading eyebrow="SUNRISE CONCIERGE" title="Hỏi về khách sạn" description="Hỏi lễ tân AI về giờ nhận phòng, tiện ích hoặc địa chỉ khách sạn." badge="HỖ TRỢ BỞI AI"/><form onSubmit={e=>{e.preventDefault(); if(question.trim()) run(async()=>(await hotelApi.ask(question.trim())).answer)}}><label htmlFor="question">Câu hỏi của bạn</label><textarea id="question" required rows="3" value={question} onChange={e=>setQuestion(e.target.value)} placeholder="Ví dụ: Khách sạn có hồ bơi không?"/><div className="actions"><Submit loading={loading}>Gửi câu hỏi</Submit></div></form><Result result={result} loading={loading}/><div className="hint"><strong>Gợi ý câu hỏi</strong><div className="chips">{suggestions.map((s,i)=><button className="chip" type="button" key={s} onClick={()=>setQuestion(questions[i])}>{s}</button>)}</div></div></section>;
}
