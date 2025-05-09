import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { saveAuthTokens } from '../auth';
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
        if (!data.token || !data.refreshToken) {
          throw new Error('Thông tin xác thực không đầy đủ.');
        }
        await saveAuthTokens(data.token, data.refreshToken);
        localStorage.setItem('user', JSON.stringify({ email: data.email, hoTen: data.hoTen }));
        setMessage('Đăng nhập thành công!');
        navigate('/Dashboard');
      } else {
        setMessage(data.Message || 'Đăng nhập thất bại. Vui lòng kiểm tra email và mật khẩu.');
      }
    } catch (error) {
      setMessage('Không thể kết nối tới hệ thống. Vui lòng thử lại sau.');
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