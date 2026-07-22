import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useRef } from "react";
import { useParams } from "react-router";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import api from "../../services/api";
import "./FlashcardContextsPanel.scss";
import { Volume2, Plus } from "lucide-react";

export default function FlashcardContextsPanel({
  isOpen,
  onClose,
  card,
  handleSpeak,
}) {
  const { id: setId } = useParams();
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ["flashcardContexts", setId, card.id],
    queryFn: () =>
      api
        .get(`/sets/${setId}/flashcards/${card.id}/contexts`)
        .then((res) => res.data),
    enabled: !!card.id && isOpen,
  });

  // useMutation is Post version of useQuery
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

  return (
    <ModalWrapper isOpen={isOpen} onClose={onClose} showCloseButton={true}>
      <div className="contexts-wrapper">
        <button className="add-new-button">
          <Plus size={24} />
        </button>

        {isLoading || generateMutation.isPending ? (
          <p>Завантаження...</p>
        ) : isEmpty ? (
          <p>Не вдалося знайти або згенерувати контексти. Додайте новий.</p>
        ) : (
          <div className="contexts-scroll-container">
            {data?.map((context) => (
              <div key={context.id} className="context-block">
                {/* we get html string in response from backend (only <strong> tag allowed) */}
                <div dangerouslySetInnerHTML={{ __html: context.sentence }} />
                <button
                  type="button"
                  className="card-tile__speak card-tile__icon"
                  onClick={(e) =>
                    handleSpeak(e, cleanHtmlForSpeech(context.sentence))
                  }
                  aria-label="Прослухати вимову"
                >
                  <Volume2 size={18} />
                </button>
                <hr />
                {/* we get html string in response from backend (only <strong> tag allowed) */}
                <div
                  dangerouslySetInnerHTML={{ __html: context.translation }}
                />
              </div>
            ))}
          </div>
        )}
      </div>
    </ModalWrapper>
  );
}
