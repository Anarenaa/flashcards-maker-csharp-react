import { useEffect, useState } from "react";
import { NavLink, useLocation, useNavigate } from "react-router";
import { Menu, X } from "lucide-react";
import api from "../services/api";
import "./Header.scss";

export default function Header({ currentUser }) {
  const [isHamburgerMenuActive, setIsHamburgerMenuActive] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    setIsHamburgerMenuActive(false);
  }, [location.pathname]);
  
  const handleLogout = async (e) => {
    e.preventDefault();
    try {
      await api.post('/auth/logout');
      window.location.href = "/";
    } catch (err) {
      console.error("Помилка при виході:", err);
    }
  };
  
  function handleHamburgerMenuToggle(){
    setIsHamburgerMenuActive(!isHamburgerMenuActive);
  }

  return (
    <header className={`header ${isHamburgerMenuActive ? "header--open" : ""}`}>
      <div className="header__flex-container">
        <h1 className="header__title">Flashcards Maker</h1>
        <button className={`header__hamburger-menu ${isHamburgerMenuActive ? "active" : ""}`} 
          onClick={handleHamburgerMenuToggle}
        >
          {isHamburgerMenuActive ?
            <X/>
            : <Menu />
          }
        </button>
      </div>
      <div className="header__line"></div>
      <nav className="header__nav">
        <ul className="header__menu-list">
          {currentUser?.role === "Admin" && (
            <>
              <li className="header__nav-btn">
                <NavLink to="/admin/complaints" className="header__link">Скарги</NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/admin/users" className="header__link">Користувачі</NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/admin/categories" className="header__link">Категорії</NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/admin/settings" className="header__link">Налаштування</NavLink>
              </li>
            </>
          )}

          {currentUser?.role === "User" && (
            <>
              <li className="header__nav-btn">
                <NavLink to="/main" className="header__link">Головна</NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/my-sets" className="header__link">Мої сети</NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/my-collections" className="header__link">Мої колекції</NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/my-profile" className="header__link header__link--profile">
                  <div className="header__nav-avatar-mini">
                    {currentUser?.avatarUrl ? (
                      <img src={currentUser.avatarUrl} alt="Avatar" />
                    ) : (
                      <span>👤</span>
                    )}
                  </div>
                  Профіль
                </NavLink>
              </li>
              <li className="header__nav-btn">
                <NavLink to="/settings" className="header__link">Налаштування</NavLink>
              </li>
            </>
          )}
        </ul>

        {currentUser && (
          <form className="header__logout-form" onSubmit={handleLogout}>
            <button type="submit" className="header__btn-exit">
              Вийти
            </button>
          </form>
        )}
      </nav>
    </header>
  );
}