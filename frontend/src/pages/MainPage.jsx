import { useNavigate } from "react-router";
import Header from "../components/Header";
import "./MainPage.scss";
import { useEffect } from "react";
import SetCard from "../components/SetCard";

export default function MainPage({ currentUser }) {
  // const navigate = useNavigate();
  // useEffect(()=>{
  //     if(currentUser === null){
  //         navigate('/login');
  //     }
  // }, [currentUser, navigate]);

  // if (!currentUser) return null;

  const mockSet = {
    id: "6b29fc40-ca47-1067-b31d-00dd010f667f",
    name: "Історія алхімії та міфи про Голема",
    description:
      "Огляд праць Парацельса, зародження ранньої хімії в Празі та легенди про створення штучного життя.",
    userName: "anastasiia_m",
    authorAvatar: "https://api.dicebear.com/7.x/bottts/svg?seed=anastasiia", // симпатична тимчасова аватарка
    progress: 65,
    flashcardsCount: 24,
    isPublic: true,
    createdAt: "2026-07-06T12:00:00Z",
  };
  return (
    <>
      <h1>Всі навчальні сети</h1>

      <div className="sets-grid">
        <SetCard
          set={mockSet}
          isMine={false}
        />
      </div>
    </>
  );
}
