import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import './LoginRegister.css';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage('');

    const payload = { Email: email, MatKhau: password };

    try {
      const response = await fetch('http://localhost:5189/api/Auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      const data = await response.json();

      if (response.ok) {
        localStorage.setItem('token', data.Token);
        localStorage.setItem('refreshToken', data.RefreshToken);
        localStorage.setItem('user', JSON.stringify({ email: data.Email, hoTen: data.HoTen }));
        setMessage('Đăng nhập thành công!');
        navigate('/Dashboard'); // Chuyển hướng đến dashboard
      } else {
        setMessage(data.Message || 'Đã có lỗi xảy ra.');
      }
    } catch (error) {
      setMessage('Lỗi kết nối tới server.');
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-box">
        <h2 className="auth-title">Đăng Nhập</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="form-input"
            />
          </div>
          <div className="form-group">
            <label htmlFor="password">Mật Khẩu</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="form-input"
            />
          </div>
          <button type="submit" className="auth-button">
            Đăng Nhập
          </button>
        </form>
        {message && <p className="auth-message">{message}</p>}
      </div>
    </div>
  );
};

export default Login;