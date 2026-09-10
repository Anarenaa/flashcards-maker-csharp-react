import { useState, useEffect } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router";
import z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { toFormData } from "axios";
import { ModalWrapper } from "../../components/Shared/ModalWrapper";
import { useSetTypes } from "../../hooks/useSetTypes";
import { handleServerErrors } from "../../utils/formHandlers";
import { LANGUAGES } from "../../constants/languages";
import { ArrowRight, Check, Plus, X } from "lucide-react";
import Stepper from "../../components/Shared/Stepper";
import ToggleSwitch from "../../components/Shared/ToggleSwitch";
import api from "../../services/api";
import "./SetForm.scss";

const setSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Введіть назву сету")
    .min(2, "Мінімальна кількість символів - 2")
    .max(100, "Занадто багато символів"),
  description: z.string().max(500, "Занадто багато символів"),
  type: z.coerce.number(),
  isPublic: z.boolean(),
  toLang: z.string().min(1, "Виберіть мову"),
  fromLang: z.string().optional(),
});

const setAiSchema = z.object({
  type: z.coerce.number(),
  fromLang: z.string().optional(),
  toLang: z.string().min(1, "Виберіть мову"),
  prompt: z.string().min(1, "Введіть промт").max(1000, "Забагато символів"),
  file: z.instanceof(File).optional().nullable(),
  cardsCount: z.coerce
    .number()
    .int()
    .min(1, "Не менше однієї картки")
    .optional()
    .nullable(),
});

export default function SetForm({ isOpen, onClose, initialData, isTheOnlyUserMode }) {
  const [isAiEnabled, setIsAiEnabled] = useState(false);
  const queryClient = useQueryClient();
  const types = useSetTypes().slice().reverse();

  const defaultToLang = "uk";
  const defaultFromLang = defaultToLang === "uk" ? "en" : "uk";
  const [cardsCount, setCardsCount] = useState(10);
  const [isFileActive, setIsFileActive] = useState(false);
  const [isLimitActive, setIsLimitActive] = useState(false);
  const [fileName, setFileName] = useState("Файл не вибрано");
  const [generatedCards, setGeneratedCards] = useState(null);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(isAiEnabled ? setAiSchema : setSchema),
    values: initialData
      ? {
          type: initialData.type,
          name: initialData.name,
          description: initialData.description,
          isPublic: initialData.isPublic,
          toLang: initialData.toLang,
          fromLang: initialData.fromLang,
        }
      : {
          type: types[0]?.id ?? 0,
          name: "",
          description: "",
          isPublic: false,
          toLang: defaultToLang,
          fromLang: defaultFromLang,
          prompt: "",
          file: null,
          cardsCount: undefined,
        },
  });

  const currentSetType = watch("type");

  const createSetMutation = useMutation({
    mutationFn: ({ formData, isGenerated }) =>
      api
        .post(`/my-sets`, formData, {
          params: { isGenerated },
        })
        .then((res) => res.data),
  });

  const createFlashcardsBatchMutation = useMutation({
    mutationFn: ({ setId, cards }) =>
      api.post(`sets/${setId}/flashcards/batch`, cards),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, formData }) => api.put(`/my-sets/${id}`, formData),
    onSuccess: () => {
      queryClient.invalidateQueries(["sets"]);
      reset();
      onClose();
    },
  });

  const generateMutation = useMutation({
    mutationFn: (formData) =>
      api.post(`my-sets/generate`, formData).then((res) => res.data), // .then((res) => res.data) is needed because we work with data output in onSuccess: (data)
    onSuccess: (data) => {
      setIsAiEnabled(false);
      setIsFileActive(false);
      setIsLimitActive(false);
      setFileName("Файл не вибрано");
      setCardsCount(10);
      reset();

      setValue("type", data.setInfo.type);
      setValue("toLang", data.setInfo.toLang);
      setValue("fromLang", data.setInfo.fromLang);
      
      setValue("name", data.setInfo.name);
      setValue("description", data.setInfo.description);
      setValue("isPublic", data.setInfo.isPublic);

      setGeneratedCards(data.flashcards);
    },
  });

  const onSubmit = async (data) => {
    // we need await calls in order to catch error in any if else case. Otherwise we would be using onError in every mutation
    try {
      // fromLang = toLang in subject sets
      const formattedFromLang =
        Number(data.type) === 0 ? data.fromLang : data.toLang;

      const payload = {
        ...data,
        fromLang: formattedFromLang,
      };

      if (isAiEnabled) {
        // Wrap validated form data into FormData to support multipart/like file uploads
        // and match the [FromForm] expectation on the backend.
        const formData = new FormData();

        const aiPayload = {
          type: data.type,
          toLang: data.toLang,
          fromLang: formattedFromLang,
          prompt: data.prompt,
          file: isFileActive && data.file instanceof File ? data.file : null,
          cardsCount: isLimitActive ? data.cardsCount : null,
        };

        // autoconvert to FormData (appends), ignores null/undefined
        toFormData(aiPayload, formData);

        await generateMutation.mutateAsync(formData);
      } else if (initialData) {
        await updateMutation.mutateAsync({
          id: initialData.id,
          formData: payload,
        });
      } else {
        const isGenerated = Boolean(generatedCards);

        const newSet = await createSetMutation.mutateAsync({
          formData: payload,
          isGenerated,
        });

        if (isGenerated && generatedCards?.length > 0) {
          await createFlashcardsBatchMutation.mutateAsync({
            setId: newSet.id,
            cards: generatedCards,
          });
        }

        // onSuccess
        queryClient.invalidateQueries(["sets"]);
        reset();
        onClose();
      }
    } catch (error) {
      handleServerErrors(error, setError);
    }
  };

  const isPending =
    isSubmitting || createSetMutation.isPending || updateMutation.isPending;

  return (
    <ModalWrapper
      isOpen={isOpen}
      onClose={onClose}
      showCloseButton={true}
      size="sm"
    >
      <div className="set-form">
        <div className="top-panel">
          <h2 className="title">
            {initialData ? "Редагування сету" : "Новий сет"}
          </h2>
          {!initialData && (
            <ToggleSwitch
              id="ai-checkbox-input"
              className="ai-switch"
              checked={isAiEnabled}
              onChange={(e) => setIsAiEnabled(e.target.checked)}
              label="ШІ"
              labelPosition="right"
            />
          )}
        </div>
        <form onSubmit={handleSubmit(onSubmit)}>
          {(!generatedCards || isAiEnabled) && (
            <div className="form-field">
              <label htmlFor="set-type-select">
                Що вивчаємо <span className="require">*</span>
              </label>
              <select
                id="set-type-select"
                className="sort-select"
                {...register("type", { valueAsNumber: true })}
              >
                {types.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
            </div>
          )}

          {(!generatedCards || isAiEnabled) && (
            <div className="languages">
              {Number(currentSetType) === 0 && (
                <>
                  <div className="form-field">
                    <label htmlFor="from-lang-select">
                      Мова термінів <span className="require">*</span>
                    </label>
                    <select
                      id="from-lang-select"
                      className="sort-select"
                      {...register("fromLang")}
                    >
                      {LANGUAGES.map((lang) => (
                        <option key={lang.code} value={lang.code}>
                          {lang.name}
                        </option>
                      ))}
                    </select>
                  </div>
                  <ArrowRight size={20} className="arrow-icon" />
                </>
              )}
              <div className="form-field">
                <label htmlFor="to-lang-select">
                  {Number(currentSetType) === 0
                    ? "Мова означень"
                    : "Мова сету "}
                  <span className="require">*</span>
                </label>
                <select
                  id="to-lang-select"
                  className="sort-select"
                  {...register("toLang")}
                >
                  {LANGUAGES.map((lang) => (
                    <option key={lang.code} value={lang.code}>
                      {lang.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          )}

          {!isAiEnabled && (
            <div className="form-field">
              <label htmlFor="set-name-input">
                Назва сету <span className="require">*</span>
              </label>
              <input
                type="text"
                id="set-name-input"
                {...register("name")}
                className={errors.name ? "error-input" : ""}
              />
              {errors.name && (
                <span className="text-danger">{errors.name.message}</span>
              )}
            </div>
          )}
          {!isAiEnabled && (
            <div className="form-field">
              <label htmlFor="set-description-input">Опис сету</label>
              <textarea
                id="set-description-input"
                {...register("description")}
                className={errors.description ? "error-input" : ""}
              ></textarea>
              {errors.description && (
                <span className="text-danger">
                  {errors.description.message}
                </span>
              )}
            </div>
          )}

          {!isAiEnabled && generatedCards && generatedCards.length > 0 && (
            <div className="generated-cards">
              {generatedCards.map((card, i) => (
                <div key={i} className="generated-card">
                  <div className="generated-card__content">
                    <span className="generated-card__term">{card.term} </span>
                    <span className="generated-card__description">
                      - {card.definition}
                    </span>
                  </div>
                  <button
                    type="button"
                    className="generated-card__remove-btn"
                    onClick={() =>
                      setGeneratedCards((prev) =>
                        prev.filter((_, index) => index !== i),
                      )
                    }
                  >
                    <X className="icon" size={18} />
                  </button>
                </div>
              ))}
            </div>
          )}

          {isAiEnabled && (
            <div className="form-field">
              <label htmlFor="set-promt-input">
                Промт для генерації <span className="require">*</span>
              </label>
              <textarea
                id="set-promt-input"
                {...register("prompt")}
                className={errors.prompt ? "error-input" : ""}
                placeholder={
                  Number(currentSetType) === 0
                    ? "Наприклад: Корисні розмовні фрази для замовлення їжі в ресторані, рівня B1"
                    : "Наприклад: Основні поняття та терміни з біології про клітинне дихання"
                }
              ></textarea>
              {errors.prompt && (
                <span className="text-danger">{errors.prompt.message}</span>
              )}
            </div>
          )}
          {!isAiEnabled && !isTheOnlyUserMode && (
            <ToggleSwitch
              id="is-public-checkbox-input"
              className="is-public-switch"
              registerProps={register("isPublic")}
              label="Публічний"
              labelPosition="left"
            />
          )}
          {isAiEnabled && (
            <div className="ai-controls">
              <div className="control-row">
                <label className="custom-checkbox">
                  <input
                    type="checkbox"
                    checked={isFileActive}
                    onChange={(e) => {
                      const checked = e.target.checked;
                      setIsFileActive(checked);
                      if (!checked) {
                        setValue("file", null);
                        setFileName("Файл не вибрано");
                      }
                    }}
                  />
                  <Check className="checkmark" size={18} />
                </label>

                <label
                  className={`file-btn ${!isFileActive ? "disabled" : ""}`}
                >
                  <Plus size={20} />
                  <input
                    type="file"
                    disabled={!isFileActive}
                    accept=".png,.jpg,.jpeg,.webp,.pdf,.txt,.docx,.doc,"
                    {...register("file")}
                    onChange={(e) => {
                      const file = e.target.files[0];
                      if (!file) {
                        setValue("file", null);
                        setFileName("Файл не вибрано");
                      } else {
                        setValue("file", file);
                        setFileName(file.name);
                      }
                    }}
                  />
                  <span className={!isFileActive ? "disabled" : ""}>
                    <span className="file-name">{fileName}</span>
                  </span>
                </label>
              </div>

              <div className="control-row">
                <label className="custom-checkbox">
                  <input
                    type="checkbox"
                    checked={isLimitActive}
                    onChange={(e) => {
                      const checked = e.target.checked;
                      setIsLimitActive(checked);
                      setValue("cardsCount", checked ? cardsCount : null);
                    }}
                  />
                  <Check className="checkmark" size={18} />
                </label>

                <div className={!isLimitActive ? "disabled" : ""}>
                  <Stepper
                    value={cardsCount}
                    onChange={(val) => {
                      setCardsCount(val);
                      if (isLimitActive) {
                        setValue("cardsCount", val);
                      }
                    }}
                  />
                </div>
              </div>
            </div>
          )}

          {!isAiEnabled && (
            <button
              type="submit"
              className="primary-button"
              disabled={isPending}
            >
              {isPending ? "Збереження..." : "Зберегти"}
            </button>
          )}
          {isAiEnabled && (
            <button
              type="submit"
              className="gradient-button set-form-generate-button"
              disabled={isPending}
            >
              {isPending ? "Генерація..." : "Згенерувати"}
            </button>
          )}
        </form>
      </div>
    </ModalWrapper>
  );
}
