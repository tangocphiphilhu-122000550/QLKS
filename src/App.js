import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LoginRegister from './Component/LoginRegister';
import Dashboard from './Component/Dashboard';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LoginRegister />} />
        <Route path="/Dashboard" element={<Dashboard />} />
      </Routes>
    </Router>
  );
}

export default App;