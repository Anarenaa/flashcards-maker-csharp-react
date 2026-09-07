import { Loader2, Lightbulb } from "lucide-react";
import { ModalWrapper } from "../../components/Shared/ModalWrapper.jsx";
import z from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { handleServerErrors } from "../../utils/formHandlers.js";
import api from "../../services/api.jsx";
import "./FlashcardForm.scss";

const flashcardSchema = z.object({
  term: z.string().min(1, "Введіть термін").max(100, "Забагато символів"),
  definition: z
    .string()
    .min(1, "Введіть визначення")
    .max(500, "Забагато символів"),
});

export default function FlashcardForm({
  isOpen,
  onClose,
  setId,
  totalItems,
  pageSize,
  handlePageChange,
  initialData,
}) {
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(flashcardSchema),
    values: initialData
      ? {
          term: initialData.term,
          definition: initialData.definition,
        }
      : {
          term: "",
          definition: "",
        },
  });
  const termValue = watch("term");

  const createMutation = useMutation({
    mutationFn: (formData) =>
      api.post(`/sets/${setId}/flashcards`, formData).then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries(["setCards", setId]);
      reset();
      onClose();
      
      const lastPage = Math.ceil(((totalItems ?? 0) + 1) / pageSize);
      handlePageChange(lastPage);
    },
  });

  const hintMutation = useMutation({
    mutationFn: (term) =>
      api
        .post(`sets/${setId}/flashcards/hint`, null, { params: { term } }) // null is instead of RequestBody because backend accepts only params (FromQuery)
        .then((res) => res.data),
    onSuccess: (hintText) => {
      setValue("definition", hintText);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, formData }) =>
      api.put(`/sets/${setId}/flashcards/${id}`, formData),
    onSuccess: () => {
      queryClient.invalidateQueries(["setCards"]);
      reset();
      onClose();
    },
  });

  const onSubmit = async (data) => {
    try {
      if (initialData) {
        await updateMutation.mutateAsync({
          id: initialData.id,
          formData: data,
        });
      } else {
        await createMutation.mutateAsync(data);
      }
    } catch (error) {
      handleServerErrors(error, setError);
    }
  };

  const isPending =
    isSubmitting || createMutation.isPending || updateMutation.isPending;

  return (
    <ModalWrapper
      isOpen={isOpen}
      onClose={onClose}
      showCloseButton={true}
      size="sm"
    >
      <div className="top-panel">
        <h2 className="title">
          {initialData ? "Редагування картки" : "Нова картка"}
        </h2>
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="form-field">
          <label htmlFor="term-input">Термін</label>
          <input
            type="text"
            id="term-input"
            tabIndex={1}
            {...register("term")}
            className={errors.term ? "error-input" : ""}
          />
          {errors.term && (
            <span className="text-danger">{errors.term.message}</span>
          )}
        </div>
        <div className="form-field">
          <label htmlFor="definition-input">Визначення</label>
          <div className="definition-input-container">
            <textarea
              id="definition-input"
              tabIndex={2}
              {...register("definition")}
              className={errors.definition ? "error-input" : ""}
            />
            <button
              type="button"
              className="definition-hint-button icon"
              title="Підказка"
              tabIndex={3}
              onClick={() => hintMutation.mutate(termValue)}
              disabled={
                !termValue || termValue.trim() === "" || hintMutation.isPending
              }
            >
              {hintMutation.isPending ? (
                <Loader2 size={20} className="spinner-icon" />
              ) : (
                <Lightbulb size={20} />
              )}
            </button>
          </div>
          {errors.definition && (
            <span className="text-danger">{errors.definition.message}</span>
          )}
        </div>
        <button
          type="submit"
          tabIndex={4}
          className="primary-button"
          disabled={isPending}
        >
          {isPending ? "Збереження..." : "Зберегти"}
        </button>
      </form>
    </ModalWrapper>
  );
}
