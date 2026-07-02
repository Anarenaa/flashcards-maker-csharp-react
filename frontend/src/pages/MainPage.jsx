import { useNavigate } from 'react-router';
import Header from '../components/Header';
import './MainPage.scss';
import { useEffect } from 'react';

export default function MainPage({currentUser}){

    // const navigate = useNavigate();
    // useEffect(()=>{
    //     if(currentUser === null){
    //         navigate('/login');
    //     }
    // }, [currentUser, navigate]);

    // if (!currentUser) return null;

    return(
        <h1>Main Page</h1>
    );
}