let isRefreshing = false;

const openDB = () => {
  return new Promise((resolve, reject) => {
    const request = indexedDB.open('AuthDB', 1);
    
    request.onupgradeneeded = (event) => {
      const db = event.target.result;
      db.createObjectStore('tokens', { keyPath: 'id' });
    };
    
    request.onsuccess = (event) => {
      resolve(event.target.result);
    };
    
    request.onerror = (event) => {
      reject(event.target.error);
    };
  });
};

const getToken = async () => {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['tokens'], 'readonly');
    const store = transaction.objectStore('tokens');
    const request = store.get('auth');
    
    request.onsuccess = () => {
      resolve(request.result || { token: undefined, refreshToken: undefined });
    };
    
    request.onerror = () => {
      reject(request.error);
    };
  });
};

const saveToken = async (token, refreshToken) => {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['tokens'], 'readwrite');
    const store = transaction.objectStore('tokens');
    const request = store.put({ id: 'auth', token, refreshToken });
    
    request.onsuccess = () => {
      resolve();
    };
    
    request.onerror = (event) => {
      reject(event.target.error);
    };
  });
};

const clearTokens = async () => {
  const db = await openDB();
  return new Promise((resolve, reject) => {
    const transaction = db.transaction(['tokens'], 'readwrite');
    const store = transaction.objectStore('tokens');
    const request = store.delete('auth');
    
    request.onsuccess = () => {
      resolve();
    };
    
    request.onerror = () => {
      reject(request.error);
    };
  });
};

const refreshToken = async (oldToken, oldRefreshToken) => {
  if (isRefreshing) {
    throw new Error('Hệ thống đang xử lý. Vui lòng thử lại sau.');
  }

  isRefreshing = true;
  try {
    const refreshResponse = await fetch('http://localhost:5189/api/Auth/refresh-token', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        Token: oldToken,
        RefreshToken: oldRefreshToken,
      }),
    });

    const responseText = await refreshResponse.text();

    if (!refreshResponse.ok) {
      throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
    }

    let refreshData;
    try {
      refreshData = JSON.parse(responseText);
    } catch (parseError) {
      throw new Error('Không thể xử lý dữ liệu từ hệ thống. Vui lòng thử lại.');
    }

    if (!refreshData.token || !refreshData.refreshToken) {
      throw new Error('Thông tin xác thực không đầy đủ. Vui lòng đăng nhập lại.');
    }

    await saveToken(refreshData.token, refreshData.refreshToken);

    const newTokens = await getToken();

    if (!newTokens.token) {
      throw new Error('Không thể cập nhật phiên đăng nhập. Vui lòng đăng nhập lại.');
    }

    return newTokens;
  } catch (refreshError) {
    throw refreshError;
  } finally {
    isRefreshing = false;
  }
};

export const apiFetch = async (url, options = {}) => {
  try {
    const tokens = await getToken();

    if (!tokens.token) {
      throw new Error('Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.');
    }

    const headers = {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${tokens.token}`,
      ...options.headers,
    };

    let response = await fetch(url, { ...options, headers });

    if (response.status === 401 && tokens.refreshToken) {
      const newTokens = await refreshToken(tokens.token, tokens.refreshToken);
      headers.Authorization = `Bearer ${newTokens.token}`;
      response = await fetch(url, { ...options, headers }); // Gọi lại API với token mới
    }

    return response; // Trả về response sau khi xử lý, đảm bảo phản ánh trạng thái thực tế
  } catch (error) {
    throw error;
  }
};

export const saveAuthTokens = saveToken;
export const clearAuthTokens = clearTokens;