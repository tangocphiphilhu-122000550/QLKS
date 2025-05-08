import React from 'react';
import { useNavigate } from 'react-router-dom';
import './Dashboard.css';

const Dashboard = () => {
  const navigate = useNavigate();
  
  // Lấy thông tin người dùng từ localStorage
  const userString = localStorage.getItem('user');
  const user = userString ? JSON.parse(userString) : { email: 'guest@example.com', hoTen: 'Guest' };
  
  // Lấy chữ cái đầu của tên người dùng cho avatar
  const initials = user?.hoTen?.charAt(0)?.toUpperCase() || 'G';
  
  // Xử lý đăng xuất
  const handleSignOut = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    navigate('/login');
  };

  // Dữ liệu phòng (có thể thay thế bằng dữ liệu từ API)
  const rooms = [
    { id: 11, status: 'available' },
    { id: 12, status: 'available' },
    { id: 13, status: 'occupied' },
    { id: 14, status: 'available' },
    { id: 15, status: 'maintenance' },
    { id: 16, status: 'occupied' },
    { id: 17, status: 'available' },
    { id: 18, status: 'available' },
    { id: 19, status: 'occupied' },
    { id: 210, status: 'maintenance' },
    { id: 21, status: 'available' },
    { id: 22, status: 'occupied' },
    { id: 23, status: 'available' },
    { id: 24, status: 'available' },
    { id: 25, status: 'maintenance' },
    { id: 26, status: 'available' },
    { id: 27, status: 'available' },
    { id: 28, status: 'occupied' },
    { id: 29, status: 'available' },
    { id: 310, status: 'maintenance' }
  ];

  // Tính toán số liệu thống kê
  const checkInsToday = 0;
  const checkOutsToday = 0;
  const availableRooms = rooms.filter(room => room.status === 'available').length;
  const occupiedRooms = rooms.filter(room => room.status === 'occupied').length;

  return (
    <div className="app-container">
      {/* Sidebar */}
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

      {/* Main Content */}
      <div className="main-content">
        <div className="page-header">
          <h1 className="page-title">Dashboard</h1>
          <div className="page-subtitle">Welcome back, {user.hoTen}</div>
        </div>

        {/* Stats Cards */}
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

        {/* Room Status Section */}
        <div className="section">
          <div className="section-header">
            <div className="section-title">Room Status</div>
            <div className="status-legend">
              <div className="legend-item">
                <div className="legend-dot available"></div>
                Available
              </div>
              <div className="legend-item">
                <div className="legend-dot occupied"></div>
                Occupied
              </div>
              <div className="legend-item">
                <div className="legend-dot maintenance"></div>
                Maintenance
              </div>
            </div>
          </div>

          <div className="room-grid">
            {rooms.slice(0, 12).map(room => (
              <div key={room.id} className={`room-cell ${room.status}`}>
                {room.id}
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

        {/* Recent Bookings */}
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