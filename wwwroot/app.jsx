const { useState, useEffect } = React;

const currencyFormatter = new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
});

function App() {
    const currentYear = new Date().getFullYear();
    const [fromDate, setFromDate] = useState(`${currentYear}-01-01`);
    const [toDate, setToDate] = useState(`${currentYear}-12-31`);
    
    // Report state
    const [reportData, setReportData] = useState({ totalBookings: 0, totalRevenue: 0, items: [] });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);

    // AI Chat state
    const [askQuestion, setAskQuestion] = useState('');
    const [askMessages, setAskMessages] = useState([
        { sender: 'ai', text: 'Xin chào! Tôi là lễ tân tại Sunrise Hotel Nha Trang. Quý khách cần thông tin gì về check-in, ăn sáng, hồ bơi hay giá phòng ạ?' }
    ]);
    const [asking, setAsking] = useState(false);

    const [chatMessage, setChatMessage] = useState('');
    const [assistantMessages, setAssistantMessages] = useState([
        { sender: 'ai', text: 'Chào quý khách! Tôi có thể kiểm tra phòng trống thực tế, tra cứu đơn đặt theo SĐT hoặc hỗ trợ hủy phòng giúp quý khách.' }
    ]);
    const [chatting, setChatting] = useState(false);

    // Fetch report on mount
    useEffect(() => {
        fetchReport();
    }, []);

    const fetchReport = async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await fetch(`/api/reports/bookings?from=${encodeURIComponent(fromDate)}&to=${encodeURIComponent(toDate)}`);
            if (!res.ok) {
                const errJson = await res.json().catch(() => ({}));
                throw new Error(errJson.error || `Lỗi HTTP ${res.status}`);
            }
            const data = await res.json();
            setReportData(data);
        } catch (err) {
            setError(err.message || 'Không thể tải báo cáo từ máy chủ.');
        } finally {
            setLoading(false);
        }
    };

    const handleFilterSubmit = (e) => {
        e.preventDefault();
        fetchReport();
    };

    const handleAskSubmit = async (e) => {
        e.preventDefault();
        const q = askQuestion.trim();
        if (!q || asking) return;

        setAskMessages(prev => [...prev, { sender: 'user', text: q }]);
        setAskQuestion('');
        setAsking(true);

        try {
            const res = await fetch('/api/ai/ask', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ question: q })
            });
            const data = await res.json();
            setAskMessages(prev => [...prev, { sender: 'ai', text: data.answer || data.error || 'Không có phản hồi.' }]);
        } catch (err) {
            setAskMessages(prev => [...prev, { sender: 'ai', text: 'Lỗi kết nối đến máy chủ.' }]);
        } finally {
            setAsking(false);
        }
    };

    const handleAssistantSubmit = async (e) => {
        e.preventDefault();
        const msg = chatMessage.trim();
        if (!msg || chatting) return;

        setAssistantMessages(prev => [...prev, { sender: 'user', text: msg }]);
        setChatMessage('');
        setChatting(true);

        try {
            const res = await fetch('/api/assistant/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: msg })
            });
            const data = await res.json();
            setAssistantMessages(prev => [...prev, { sender: 'ai', text: data.reply || data.error || 'Không có phản hồi.' }]);
            // Re-fetch report if assistant made any changes (like cancelling booking)
            fetchReport();
        } catch (err) {
            setAssistantMessages(prev => [...prev, { sender: 'ai', text: 'Lỗi kết nối đến máy chủ.' }]);
        } finally {
            setChatting(false);
        }
    };

    return (
        <div>
            {/* Header Banner */}
            <header className="hero-banner">
                <div className="container text-center">
                    <h1 className="display-6 fw-bold"><i className="bi bi-building"></i> Sunrise Hotel</h1>
                    <p className="mb-0 text-light opacity-75">25 Tran Phu, Nha Trang | Báo cáo Đặt phòng & Trợ lý Thông minh AI (React UI)</p>
                </div>
            </header>

            <main className="container pb-5">
                {/* KPI Cards */}
                <div className="row g-3 mb-4">
                    <div className="col-md-6">
                        <div className="stat-card d-flex align-items-center">
                            <div className="rounded-circle bg-primary bg-opacity-10 p-3 me-3 text-primary">
                                <i className="bi bi-calendar-check fs-2"></i>
                            </div>
                            <div>
                                <div className="stat-label">Tổng Đơn Đặt (Confirmed)</div>
                                <div className="stat-value">{reportData.totalBookings || 0}</div>
                            </div>
                        </div>
                    </div>
                    <div className="col-md-6">
                        <div className="stat-card d-flex align-items-center">
                            <div className="rounded-circle bg-success bg-opacity-10 p-3 me-3 text-success">
                                <i className="bi bi-cash-stack fs-2"></i>
                            </div>
                            <div>
                                <div className="stat-label">Tổng Doanh Thu</div>
                                <div className="stat-value">{currencyFormatter.format(reportData.totalRevenue || 0)}</div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Booking Report Section (Requirement 5) */}
                <section className="card mb-4">
                    <div className="card-header d-flex justify-content-between align-items-center py-3">
                        <h5 className="mb-0"><i className="bi bi-table"></i> Báo cáo Đặt phòng (Booking Report)</h5>
                        <span className="badge bg-primary">React Component</span>
                    </div>
                    <div className="card-body">
                        {/* Filter Form */}
                        <form className="row g-3 align-items-end mb-4" onSubmit={handleFilterSubmit}>
                            <div className="col-md-4">
                                <label className="form-label fw-semibold">Từ ngày (From)</label>
                                <input 
                                    type="date" 
                                    className="form-control" 
                                    value={fromDate}
                                    onChange={(e) => setFromDate(e.target.value)}
                                    required 
                                />
                            </div>
                            <div className="col-md-4">
                                <label className="form-label fw-semibold">Đến ngày (To)</label>
                                <input 
                                    type="date" 
                                    className="form-control" 
                                    value={toDate}
                                    onChange={(e) => setToDate(e.target.value)}
                                    required 
                                />
                            </div>
                            <div className="col-md-4">
                                <button type="submit" className="btn btn-primary w-100 fw-semibold" disabled={loading}>
                                    {loading ? (
                                        <span><span className="spinner-border spinner-border-sm me-2"></span>Đang lọc...</span>
                                    ) : (
                                        <span><i className="bi bi-search"></i> Search / Filter</span>
                                    )}
                                </button>
                            </div>
                        </form>

                        {/* Loading State */}
                        {loading && (
                            <div className="text-center py-5">
                                <div className="spinner-border text-primary" role="status">
                                    <span className="visually-hidden">Loading...</span>
                                </div>
                                <p className="mt-2 text-muted">Đang truy vấn dữ liệu từ MySQL...</p>
                            </div>
                        )}

                        {/* Error State */}
                        {error && (
                            <div className="alert alert-danger text-center" role="alert">
                                <i className="bi bi-exclamation-triangle-fill me-2"></i> {error}
                            </div>
                        )}

                        {/* Empty State */}
                        {!loading && !error && (!reportData.items || reportData.items.length === 0) && (
                            <div className="alert alert-warning text-center py-4" role="alert">
                                <i className="bi bi-inbox fs-2 d-block mb-2"></i>
                                Không có đơn đặt phòng nào được xác nhận trong khoảng thời gian từ {fromDate} đến {toDate}.
                            </div>
                        )}

                        {/* Table State */}
                        {!loading && !error && reportData.items && reportData.items.length > 0 && (
                            <div className="table-responsive">
                                <table className="table table-hover align-middle">
                                    <thead>
                                        <tr>
                                            <th>No</th>
                                            <th>Room Number</th>
                                            <th>Room Type</th>
                                            <th>Guest Name</th>
                                            <th>Phone</th>
                                            <th>Check-in</th>
                                            <th>Check-out</th>
                                            <th>Nights</th>
                                            <th className="text-end">Total Price</th>
                                            <th>Status</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {reportData.items.map((item) => (
                                            <tr key={item.no || item.bookingId}>
                                                <td><strong>{item.no}</strong></td>
                                                <td><span className="badge bg-secondary">{item.roomNumber}</span></td>
                                                <td>{item.roomType}</td>
                                                <td>{item.guestName}</td>
                                                <td>{item.phone}</td>
                                                <td>{item.checkIn}</td>
                                                <td>{item.checkOut}</td>
                                                <td><span className="badge bg-info text-dark">{item.nights}</span></td>
                                                <td className="text-end fw-bold">{currencyFormatter.format(item.totalPrice)}</td>
                                                <td>
                                                    <span className={`badge ${item.status === 'Confirmed' ? 'badge-confirmed' : 'badge-cancelled'}`}>
                                                        {item.status}
                                                    </span>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>
                </section>

                {/* AI Interactive Testing Section */}
                <section className="row g-4">
                    {/* Ask Hotel AI (Req 1) */}
                    <div className="col-lg-6">
                        <div className="card h-100">
                            <div className="card-header py-3">
                                <h6 className="mb-0"><i className="bi bi-robot"></i> Hỏi đáp Lễ tân AI (Ask Hotel API)</h6>
                            </div>
                            <div className="card-body d-flex flex-column">
                                <div className="chat-box mb-3 flex-grow-1">
                                    {askMessages.map((msg, i) => (
                                        <div key={i} className={`chat-message ${msg.sender}`}>
                                            {msg.text}
                                        </div>
                                    ))}
                                    {asking && <div className="chat-message ai text-muted"><em>Đang suy nghĩ câu trả lời...</em></div>}
                                </div>
                                <form onSubmit={handleAskSubmit} className="input-group">
                                    <input 
                                        type="text" 
                                        className="form-control" 
                                        placeholder="Ví dụ: Does the hotel have a swimming pool?"
                                        value={askQuestion}
                                        onChange={(e) => setAskQuestion(e.target.value)}
                                        disabled={asking}
                                    />
                                    <button className="btn btn-outline-primary" type="submit" disabled={asking}>
                                        <i className="bi bi-send"></i> Hỏi
                                    </button>
                                </form>
                            </div>
                        </div>
                    </div>

                    {/* Booking Assistant (Req 3 - Function Calling) */}
                    <div className="col-lg-6">
                        <div className="card h-100">
                            <div className="card-header py-3">
                                <h6 className="mb-0"><i className="bi bi-stars"></i> Trợ lý Đặt phòng AI (Function Calling)</h6>
                            </div>
                            <div className="card-body d-flex flex-column">
                                <div className="chat-box mb-3 flex-grow-1">
                                    {assistantMessages.map((msg, i) => (
                                        <div key={i} className={`chat-message ${msg.sender}`}>
                                            {msg.text}
                                        </div>
                                    ))}
                                    {chatting && <div className="chat-message ai text-muted"><em>Đang gọi Function và truy vấn Database...</em></div>}
                                </div>
                                <form onSubmit={handleAssistantSubmit} className="input-group">
                                    <input 
                                        type="text" 
                                        className="form-control" 
                                        placeholder="Ví dụ: Còn phòng Suite nào trống từ 24/12 đến 26/12 không?"
                                        value={chatMessage}
                                        onChange={(e) => setChatMessage(e.target.value)}
                                        disabled={chatting}
                                    />
                                    <button className="btn btn-outline-success" type="submit" disabled={chatting}>
                                        <i className="bi bi-send"></i> Gửi
                                    </button>
                                </form>
                            </div>
                        </div>
                    </div>
                </section>
            </main>

            <footer className="text-center py-3 text-muted border-top bg-white">
                <small>© 2026 Sunrise Hotel Management. Powered by ASP.NET Core & Google Gemini AI (React Frontend).</small>
            </footer>
        </div>
    );
}

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(<App />);
