import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

const Layout: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    setIsAuthenticated(!!token);
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    setIsAuthenticated(false);
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div className="flex items-center">
              <Link to="/" className="text-2xl font-bold text-gray-900">
                Real Estate
              </Link>
            </div>
            <nav className="hidden md:flex space-x-10">
              <Link to="/" className="text-gray-500 hover:text-gray-900">
                Home
              </Link>
              <Link to="/properties" className="text-gray-500 hover:text-gray-900">
                Properties
              </Link>
              {isAuthenticated && (
                <>
                  <Link to="/favorites" className="text-gray-500 hover:text-gray-900">
                    Favorites
                  </Link>
                  <Link to="/profile" className="text-gray-500 hover:text-gray-900">
                    Profile
                  </Link>
                </>
              )}
            </nav>
            <div className="flex items-center space-x-4">
              {isAuthenticated ? (
                <button
                  onClick={handleLogout}
                  className="bg-red-600 text-white px-4 py-2 rounded-md hover:bg-red-700"
                >
                  Logout
                </button>
              ) : (
                <>
                  <Link to="/login" className="text-gray-500 hover:text-gray-900">
                    Login
                  </Link>
                  <Link to="/register" className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700">
                    Register
                  </Link>
                </>
              )}
            </div>
          </div>
        </div>
      </header>
      <main>{children}</main>
      <footer className="bg-white border-t">
        <div className="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
          <p className="text-center text-gray-500">© 2023 Real Estate Marketplace. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
};

export default Layout;