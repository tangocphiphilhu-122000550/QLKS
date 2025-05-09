import React, { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiFetch, clearAuthTokens } from '../auth';
import './Dashboard.css';

const Dashboard = () => {
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [rooms, setRooms] = useState([]);
  const isMounted = useRef(false);

  const userString = localStorage.getItem('user');
  const user = userString ? JSON.parse(userString) : { email: 'guest@example.com', hoTen: 'Guest' };
  const initials = user?.hoTen?.charAt(0)?.toUpperCase() || 'G';

  const checkInsToday = 0;
  const checkOutsToday = 0;
  const availableRooms = rooms.filter(room => room.trangThai === 'Trống').length;
  const occupiedRooms = rooms.filter(room => room.trangThai === 'Đang sử dụng').length;

  useEffect(() => {
    if (isMounted.current) return;
    isMounted.current = true;

    const fetchData = async () => {
      try {
        const response = await apiFetch('http://localhost:5189/api/Phong/GetAll', {
          method: 'GET',
        });

        // Chỉ ném lỗi nếu response không ok sau khi làm mới token
        if (!response.ok) {
          throw new Error('Không thể tải dữ liệu phòng.');
        }

        const result = await response.json();
        const validRooms = result.filter(room => room.maPhong && room.trangThai);
        setRooms(validRooms);
        setError(''); // Xóa lỗi nếu dữ liệu tải thành công
      } catch (err) {
        setError('Không thể tải dữ liệu. Vui lòng thử lại sau.');
      }
    };

    if (user.email !== 'guest@example.com') {
      fetchData();
    }

    return () => {
      isMounted.current = false;
    };
  }, [navigate, user.email]);

  const handleSignOut = async () => {
    try {
      const response = await apiFetch('http://localhost:5189/api/Auth/logout', {
        method: 'POST',
      });

      if (!response.ok) {
        throw new Error('Không thể đăng xuất.');
      }

      await clearAuthTokens();
      localStorage.removeItem('user');
      navigate('/login');
    } catch (error) {
      setError('Đăng xuất thất bại. Vui lòng thử lại.');
    }
  };

  return (
    <div className="app-container">
      <div className="sidebar">
        <div className="sidebar-header">
          <div className="sidebar-title">Hotel Management</div>
          <div className="sidebar-subtitle">Staff Portal</div>
        </div>
        
        <nav className="sidebar-nav">
          <div className="nav-item active">
            <span className="nav-icon">
              <i className="fas fa-home"></i>
            </span>
            Dashboard
          </div>
          <div className="nav-item">
            <span className="nav-icon">
              <i className="fas fa-calendar-check"></i>
            </span>
            Bookings
          </div>
          <div className="nav-item">
            <span className="nav-icon">
              <i className="fas fa-door-open"></i>
            </span>
            Rooms
          </div>
          <div className="nav-item">
            <span className="nav-icon">
              <i className="fas fa-users"></i>
            </span>
            Customers
          </div>
          <div className="nav-item">
            <span className="nav-icon">
              <i className="fas fa-concierge-bell"></i>
            </span>
            Services
          </div>
          <div className="nav-item">
            <span className="nav-icon">
              <i className="fas fa-file-invoice-dollar"></i>
            </span>
            Invoices
          </div>
        </nav>
        
        <div className="sidebar-footer">
          <div className="user-avatar">{initials}</div>
          <div className="user-info">
            <div className="user-name">{user.hoTen}</div>
            <div className="user-role">Reception Staff</div>
            <div className="sign-out" onClick={handleSignOut}>
              <span className="icon">
                <i className="fas fa-sign-out-alt"></i>
              </span>
              Sign Out
            </div>
          </div>
        </div>
      </div>

      <div className="main-content">
        <div className="page-header">
          <h1 className="page-title">Dashboard</h1>
          <div className="page-subtitle">Welcome back, {user.hoTen}</div>
        </div>

        {error && (
          <div
            style={{
              color: 'red',
              textAlign: 'center',
              marginBottom: '1rem',
              padding: '10px',
              backgroundColor: '#ffe6e6',
              borderRadius: '4px',
            }}
          >
            {error}
          </div>
        )}

        <div className="stats-cards">
          <div className="stat-card">
            <div className="card-info">
              <div className="card-label">Today's Check-ins</div>
              <div className="card-value">{checkInsToday}</div>
            </div>
            <div className="card-icon check-in">
              <i className="fas fa-sign-in-alt"></i>
            </div>
          </div>

          <div className="stat-card">
            <div className="card-info">
              <div className="card-label">Today's Check-outs</div>
              <div className="card-value">{checkOutsToday}</div>
            </div>
            <div className="card-icon check-out">
              <i className="fas fa-sign-out-alt"></i>
            </div>
          </div>

          <div className="stat-card">
            <div className="card-info">
              <div className="card-label">Available Rooms</div>
              <div className="card-value">{availableRooms}</div>
            </div>
            <div className="card-icon available">
              <i className="fas fa-door-open"></i>
            </div>
          </div>

          <div className="stat-card">
            <div className="card-info">
              <div className="card-label">Occupied Rooms</div>
              <div className="card-value">{occupiedRooms}</div>
            </div>
            <div className="card-icon occupied">
              <i className="fas fa-door-closed"></i>
            </div>
          </div>
        </div>

        <div className="section">
          <div className="section-header">
            <div className="section-title">Room Status</div>
            <div className="status-legend">
              <div className="legend-item">
                <div className="legend-dot available"></div>
                Trống
              </div>
              <div className="legend-item">
                <div className="legend-dot booked"></div>
                Đã đặt
              </div>
              <div className="legend-item">
                <div className="legend-dot occupied"></div>
                Đang sử dụng
              </div>
              <div className="legend-item">
                <div className="legend-dot maintenance"></div>
                Bảo trì
              </div>
            </div>
          </div>

          <div className="room-grid">
            {rooms.slice(0, 12).map(room => (
              <div
                key={room.maPhong}
                className={`room-cell ${room.trangThai?.toLowerCase().replace(' ', '-') || ''}`}
              >
                {room.trangThai === 'bảo trì' ? (
                  <>
                    {room.tenPhong}
                    <span className="maintenance-icon">!</span>
                  </>
                ) : (
                  room.tenPhong
                )}
              </div>
            ))}
          </div>

          <div className="view-all">
            <span className="view-all-icon">
              <i className="fas fa-arrow-right"></i>
            </span>
            View All Rooms
          </div>
        </div>

        <div className="section">
          <div className="section-header">
            <div className="section-title">Recent Bookings</div>
            <div className="view-all">View All</div>
          </div>

          <table className="bookings-table">
            <thead>
              <tr>
                <th>BookingID</th>
                <th>Guest</th>
                <th>Room</th>
                <th>Check In</th>
                <th>Check Out</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td colSpan="7" className="no-bookings">
                  No bookings found
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div className="content-footer">
          Content Script
        </div>
      </div>
    </div>
  );
};

export default Dashboard;