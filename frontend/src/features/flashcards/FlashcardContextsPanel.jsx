import { Pencil, Trash2, Plus } from "lucide-react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { useParams } from "react-router";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import FlashcardContextsForm from "./FlashcardContextForm";
import PronounceButton from "../../components/Shared/PronounceButton";
import api from "../../services/api";
import "./FlashcardContextsPanel.scss";

export default function FlashcardContextsPanel({ isOpen, onClose, card }) {
  const { id: setId } = useParams();
  const queryClient = useQueryClient();
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingContext, setEditingContext] = useState(null);

  const { data, isLoading } = useQuery({
    queryKey: ["flashcardContexts", setId, card.id],
    queryFn: () =>
      api
        .get(`/sets/${setId}/flashcards/${card.id}/contexts`)
        .then((res) => res.data),
    enabled: !!card.id && isOpen,
  });

  // useMutation is Post/Delete version of useQuery
  const generateMutation = useMutation({
    mutationFn: () =>
      api
        .post(`/sets/${setId}/flashcards/${card.id}/generate-context`)
        .then((res) => res.data),
    onSuccess: (newContexts) => {
      // generate-context already returns a ready-made array of contexts —
      // we simply place this response directly into the cache under the same
      // queryKey used by useQuery above. React Query
      // treats it just like data from a GET request,
      // but WITHOUT an actual re-request to the server.
      queryClient.setQueryData(
        ["flashcardContexts", setId, card.id],
        newContexts,
      );
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (contextId) =>
      api.delete(`/sets/${setId}/flashcards/${card.id}/contexts/${contextId}`),
    onSuccess: () => {
      queryClient.invalidateQueries(["flashcardContexts", setId, card.id]);
    },
  });

  const hasTriedAutoGenerate = useRef(false);
  useEffect(() => {
    hasTriedAutoGenerate.current = false;
  }, [card.id, isOpen]);

  // Autogeneration of context if there is no one in DB
  useEffect(() => {
    if (
      data &&
      data.length === 0 &&
      !hasTriedAutoGenerate.current &&
      !generateMutation.isPending
    ) {
      hasTriedAutoGenerate.current = true;
      generateMutation.mutate();
    }
  }, [data, generateMutation.isPending]);

  const isEmpty = !data || data.length === 0;

  const cleanHtmlForSpeech = (htmlString) => {
    return htmlString.replace(/<\/?[^>]+(>|$)/g, "");
  };

  const sortedData = data ? [...data].sort((a, b) => a.id - b.id) : [];

  const handleEditClick = (context) => {
    setEditingContext(context);
    setIsFormOpen(true);
  };

  const handleAddClick = () => {
    setEditingContext(null);
    setIsFormOpen(true);
  };

  return (
    <>
      <ModalWrapper isOpen={isOpen} onClose={onClose} showCloseButton={true}>
        <div className="contexts-wrapper">
          <button className="add-new-button" onClick={handleAddClick}>
            <Plus size={24} />
          </button>

          {isLoading || generateMutation.isPending ? (
            <p>Завантаження...</p>
          ) : isEmpty ? (
            <p>Не вдалося знайти або згенерувати контексти. Додайте новий.</p>
          ) : (
            <div className="contexts-scroll-container">
              {sortedData.map((context) => (
                <div key={context.id} className="context-block">
                  <div className="top-panel">
                    {/* we get html string in response from backend (only <strong> tag allowed) */}
                    <div
                      dangerouslySetInnerHTML={{ __html: context.sentence }}
                    />
                    <PronounceButton
                      word={cleanHtmlForSpeech(context.sentence)}
                      lang={card.fromLang}
                    />
                    {/* we get html string in response from backend if it exists - optional (only <strong> tag allowed) */}
                    {context.translation && (
                      <>
                        <hr />
                        <div>
                          <div
                            dangerouslySetInnerHTML={{
                              __html: context.translation,
                            }}
                          />
                        </div>
                      </>
                    )}
                  </div>
                  {!context.isGenerated && (
                    <div className="actions">
                      <button
                        aria-label="Редагувати"
                        onClick={() => handleEditClick(context)}
                      >
                        <Pencil size={15} className="icon" />
                      </button>
                      <button
                        aria-label="Видалити"
                        onClick={() => deleteMutation.mutate(context.id)}
                        disabled={deleteMutation.isPending}
                      >
                        <Trash2 size={15} className="icon" />
                      </button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </ModalWrapper>
      {isFormOpen && (
        <FlashcardContextsForm
          isOpen={isFormOpen}
          onClose={() => {
            setIsFormOpen(false);
            setEditingContext(null);
          }}
          card={card}
          initialData={editingContext}
        />
      )}
    </>
  );
}
