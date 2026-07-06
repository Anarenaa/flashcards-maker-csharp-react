import React, { useEffect, useRef } from 'react';
import { Link } from 'react-router';
import './HomePage.scss';

export default function HomePage() {
  const containerRef = useRef(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.15 }
    );

    if (containerRef.current) {
      const cards = containerRef.current.querySelectorAll('.feature-card');
      cards.forEach((card) => observer.observe(card));
    }

    return () => observer.disconnect();
  }, []);

  return (
    <div ref={containerRef} className="homepage-wrapper">

      <div className="squares-wrapper">
        {Array.from({ length: 15 }).map((_, i) => (
          <div key={i} className="square"></div>
        ))}
      </div>

      <section className="hero-viewport">
        <nav className="top-right-nav">
          <Link to="/register" className="btn">Реєстрація</Link>
          <a href="/login" className="btn">Увійти</a>
        </nav>

        <div className="hero-content">
          <h1>Flashcards Maker</h1>
          <p>Твій ідеальний простір для навчання</p>
          <div className="scroll-down">
            <a
              href="#features"
            >
              Погортати вниз ↓
            </a>
          </div>
        </div>
      </section>

      <section id="features" className="features-container">
        <div className="feature-card">
          <div className="feature-info">
            <h2>Вчися з натхненням</h2>
            <p>Твій комфорт — наш пріоритет. Налаштовуй додаток під себе: обирай готові візуальні теми або створюй власні кастомні палітри. Контролюй свій простір: залишай профіль приватним для індивідуального навчання або роби його публічним, щоб ділитися результатами з іншими.</p>
          </div>
          <div className="feature-img-wrapper">
            <img 
              src="/images/HomePage/boy-laptop.png" 
              alt="Створення"
            />
          </div>
        </div>

        <div className="feature-card">
          <div className="feature-info">
            <h2>Глибоке занурення</h2>
            <p>Опановуй нові знання без обмежень. Створюй унікальні тематичні сети карток вручну або генеруй їх за лічені секунди за допомогою ШІ — просто за описом теми чи завантаженим фото. Досліджуй чужі сети, додавай найкращі з них у власні колекції та закріплюй матеріал у зручному тренажері практики. Фокусуйся на головному — твоєму результаті навчання.</p>
          </div>
          <div className="feature-img-wrapper">
            <img 
              src="/images/HomePage/girl-tablet.png" 
              alt="Навчання"
            />
          </div>
        </div>

        <div className="feature-card">
          <div className="feature-info">
            <h2>Завжди під рукою</h2>
            <p>Твої знання завжди у твоїй кишені. Вчися в метро, у черзі чи на прогулянці — додаток ідеально працює на смартфонах, планшетах та на інших пристроях за допомогою нашої адаптації.</p>
          </div>
          <div className="feature-img-wrapper">
            <img 
              src="/images/HomePage/girl-phone.png" 
              alt="Мобільність"
            />
          </div>
        </div>
      </section>

      <footer className='footer'>
        <p>&copy; 2026 Flashcards Maker. Створено для твого успіху.</p>
      </footer>
    </div>
  );
}